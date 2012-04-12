﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CadEditor
{
    public partial class EditLayout : Form
    {
        public EditLayout()
        {
            InitializeComponent();
        }

        private void EditForm_Load(object sender, EventArgs e)
        {

            makeScreens();

            scrollSprites.Images.Clear();
            scrollSprites.Images.AddStrip(Image.FromFile("scroll_sprites//scrolls.png"));
            doorSprites.Images.Clear();
            doorSprites.Images.AddStrip(Image.FromFile("scroll_sprites//doors.png"));
            dirSprites.Images.Clear();
            dirSprites.Images.AddStrip(Image.FromFile("scroll_sprites//dirs.png"));
            objPanel.Controls.Clear();
            objPanel.SuspendLayout();

            for (int i = 0; i < scrollSprites.Images.Count; i++)
            {
                var but = new Button();
                but.Size = new Size(32, 32);
                but.ImageList = scrollSprites;
                but.ImageIndex = i;
                but.Click += new EventHandler(buttonScrollClick);
                objPanel.Controls.Add(but);
            }
            objPanel.ResumeLayout();

            doorsPanel.SuspendLayout();

            for (int i = 0; i < doorSprites.Images.Count; i++)
            {
                var but = new Button();
                but.Size = new Size(32, 32);
                but.ImageList = doorSprites;
                but.ImageIndex = i;
                but.Click += new EventHandler(buttonDoorClick);
                doorsPanel.Controls.Add(but);
            }
            doorsPanel.ResumeLayout();

            blocksPanel.Controls.Clear();
            blocksPanel.SuspendLayout();
            for (int i = 0; i < SCREENS_COUNT; i++)
            {
                var but = new Button();
                but.Size = new Size(64, 64);
                but.ImageList = screenImages;
                but.ImageIndex = i;
                but.Click += new EventHandler(buttonBlockClick);
                blocksPanel.Controls.Add(but);

            }
            blocksPanel.ResumeLayout();  
            cbLevel.SelectedIndex = 0;
        }

        private void reloadLevelLayer()
        {
            var lr =  Globals.levelData[curActiveLevel];
            int layoutAddr = lr.getActualLayoutAddr();
            int scrollAddr = lr.getActualScrollAddr();
            int dirAddr = lr.getActualDirsAddr();
            int width = lr.getWidth();
            int height = lr.getHeight();
            byte[] layer = new byte[width*height];
            byte[] scroll = new byte[width * height];
            byte[] dirs = new byte[height];
            for (int i = 0; i < width * height; i++)
            {
                layer[i] = Globals.romdata[layoutAddr + i];
                scroll[i] = Globals.romdata[scrollAddr + i];
            }
            for (int i = 0; i < height; i++)
            {
                dirs[i] = Globals.romdata[dirAddr + i];
            }
            curLevelLayerData = new LevelLayerData(width, height, layer,scroll, dirs);
            curActiveBlock = 0;
            lvObjects.Items.Clear();
            pbMap.Invalidate();
        }

        private void makeScreens()
        {
            screenImages.Images.Clear();
            screenImages.Images.Add(makeBlackScreen(64,64,0));
            for (int scrNo = 0; scrNo < SCREENS_COUNT; scrNo++)
                screenImages.Images.Add(makeBlackScreen(64, 64, scrNo + 1));
        }

        private void previewScreens(List<ScreenRec> screenList)
        {
            makeScreens();
            var sortedScreenList = new List<ScreenRec>(screenList);
            sortedScreenList.Sort((r1, r2) => { return r1.door > r2.door ? 1 : r1.door < r2.door ? -1 : 0; });
            int lastDoorNo = -1;
            ImageList smallBlocks = new ImageList();
            ImageList bigBlocks = new ImageList();
            int levelNo = curActiveLevel;
            for (int i = 0; i < sortedScreenList.Count; i++)
            {
                if (lastDoorNo != sortedScreenList[i].door)
                {
                    lastDoorNo = sortedScreenList[i].door;
                    smallBlocks.Images.Clear();
                    bigBlocks.Images.Clear();
                    byte[] bigBlockIndexes = new byte[BIG_BLOCKS_COUNT * 4];
                    //set big blocks
                    byte blockId = (byte)Globals.levelData[curActiveLevel].bigBlockId;
                    byte backId, palId;
                    if (lastDoorNo == 0)
                    {
                        backId = (byte)Globals.levelData[curActiveLevel].backId;
                        palId = (byte)Globals.levelData[curActiveLevel].palId;
                    }
                    else
                    {
                        backId = (byte)Globals.doorsData[lastDoorNo-1].backId;
                        palId = (byte)Globals.doorsData[lastDoorNo-1].palId;
                    }
                    int bigBlockAddr = Globals.getBigTilesAddr(blockId);
                    for (int btileId = 0; btileId < BIG_BLOCKS_COUNT * 4; btileId++)
                        bigBlockIndexes[btileId] = Globals.romdata[bigBlockAddr + btileId];

                    var im = Video.makeObjectsStrip(backId, blockId, palId, 1, false);
                    smallBlocks.Images.AddStrip(im);

                    //make big blocks
                    for (int btileId = 0; btileId < BIG_BLOCKS_COUNT; btileId++)
                    {
                        var b = new Bitmap(64, 64);
                        using (Graphics g = Graphics.FromImage(b))
                        {
                            g.DrawImage(smallBlocks.Images[bigBlockIndexes[btileId * 4]], new Rectangle(0, 0, 32, 32));
                            g.DrawImage(smallBlocks.Images[bigBlockIndexes[btileId * 4 + 1]], new Rectangle(31, 0, 32, 32));
                            g.DrawImage(smallBlocks.Images[bigBlockIndexes[btileId * 4 + 2]], new Rectangle(0, 31, 32, 32));
                            g.DrawImage(smallBlocks.Images[bigBlockIndexes[btileId * 4 + 3]], new Rectangle(31, 31, 32, 32));
                        }
                        bigBlocks.Images.Add(b);
                    }
                }
                int scrNo = sortedScreenList[i].no;
                if (scrNo != 0)
                {
                    byte[] indexes;
                    //level E & H hack
                    if (curActiveLevel == 5 || curActiveLevel == 8)
                        indexes = Video.getScreen(256 + scrNo - 1);
                    else
                        indexes = Video.getScreen(scrNo - 1);
                    Bitmap bitmap = new Bitmap(512, 512);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        for (int tileNo = 0; tileNo < SCREEN_SIZE; tileNo++)
                        {
                            int index = indexes[tileNo];
                            g.DrawImage(bigBlocks.Images[index], new Rectangle(tileNo % 8 * 63, tileNo / 8 * 63, 64, 64));
                        }
                        g.DrawString(String.Format("{0:X}", scrNo), new Font("Arial", 64), Brushes.White, new Point(0, 0));
                    }
                    Bitmap convertedSize = new Bitmap(64, 64);
                    using (var g = Graphics.FromImage(convertedSize))
                        g.DrawImage(bitmap, new Rectangle(0, 0, 64, 64));
                    //level E & H hack
                    if (curActiveLevel == 5 || curActiveLevel == 8)
                        screenImages.Images[scrNo + 256] = convertedSize;
                    else
                        screenImages.Images[scrNo] = convertedSize;
                }
            }
        }

        private Image makeBlackScreen(int w, int h, int no)
        {
            var b = new Bitmap(w, h);
            using (var g = Graphics.FromImage(b))
            {
                g.FillRectangle(Brushes.Black, new Rectangle(0, 0, w, h));
                g.DrawRectangle(new Pen(Color.Green, w/32), new Rectangle(0, 0, w, h));
                if (no % 256 != 0)
                  g.DrawString(String.Format("{0:X}", no), new Font("Arial", w/8), Brushes.White, new Point(0, 0));
            }
            return b;
        }

        const int SCREENS_COUNT = 300;
        const int BIG_BLOCKS_COUNT = 256;
        const int SCREEN_SIZE = 64;

      

        private void pb_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            for (int y = 0; y < curLevelLayerData.height; y++)
            {
                for (int x = 0; x < curLevelLayerData.width; x++)
                {
                    int levelEHhack = (curActiveLevel == 5 || curActiveLevel == 8) ? 256 : 0;
                    int index = curLevelLayerData.layer[y * curLevelLayerData.width + x] + levelEHhack;
                    int scrollIndex = curLevelLayerData.scroll[y * curLevelLayerData.width + x] >> 5;
                    int doorIndex = curLevelLayerData.scroll[y * curLevelLayerData.width + x] & 0x01F;
                    g.DrawImage(screenImages.Images[index], new Rectangle(x*64, y*64, 64, 64));
                    if (showScrolls)
                    {
                        g.DrawImage(scrollSprites.Images[scrollIndex], new Rectangle(x * 64 + 24, y * 64 + 24, 16, 16));
                        if (doorIndex != 0)
                            g.DrawImage(doorSprites.Images[doorIndex], new Rectangle(x * 64 + 48, y * 64 + 48, 16, 16));
                    }
                }
            }
            for (int i = 0; i < curLevelLayerData.height; i++)
            {
                int dirIndex = (curLevelLayerData.dirs[i] % 2 == 0) ? 0 : 1;
                g.DrawImage(dirSprites.Images[dirIndex], new Rectangle(curLevelLayerData.width * 64, i * 64, 64, 64));
            }
        }

        private void pb_MouseUp(object sender, MouseEventArgs e)
        {
            int dx = e.X / 64;
            int dy = e.Y / 64;
            if (dx >= curLevelLayerData.width + 1 || dy >= curLevelLayerData.height)
                return;
            dirty = true;
            if (dx == curLevelLayerData.width)
            {
                int dir = curLevelLayerData.dirs[dy];
                curLevelLayerData.dirs[dy] = (byte) ((dir == 0) ? 3 : 0);
            }
            else
            {
                int index = dy * curLevelLayerData.width + dx;

                if (drawMode == MapDrawMode.Screens)
                    curLevelLayerData.layer[index] = (byte)(curActiveBlock & 0xFF);
                else if (drawMode == MapDrawMode.Scrolls)
                    curLevelLayerData.scroll[index] = (byte)((curActiveBlock << 5) | (curLevelLayerData.scroll[index] & 0x01F));
                else if (drawMode == MapDrawMode.Doors)
                    curLevelLayerData.scroll[index] = (byte)((curActiveBlock &0x1F) | (curLevelLayerData.scroll[index] & 0xE0));
            }
            pbMap.Invalidate();
        }

        private int curActiveLevel = 0;
        private int curActiveBlock = 0;
        private MapDrawMode drawMode = MapDrawMode.Screens;
        private bool dirty = false;
        private bool showScrolls = true;
        private LevelLayerData curLevelLayerData = new LevelLayerData();
        

        private void cbLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            //TODO: refactor this block to separate method
            if (dirty)
            {
                DialogResult dr = MessageBox.Show("Level was changed. Do you want to save current level?", "Save", MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.Cancel)
                {
                    returnCbLevelIndex();
                    return;
                }
                else if (dr == DialogResult.Yes)
                {
                    if (!saveToFile())
                    {
                        returnCbLevelIndex();
                        return;
                    }
                }
                else
                {
                    dirty = false;
                }
            }
            if (cbLevel.SelectedIndex == -1)
                return;
            curActiveLevel = cbLevel.SelectedIndex;
            reloadLevelLayer();
        }

        private void buttonBlockClick(Object button, EventArgs e)
        {
            int index = ((Button)button).ImageIndex;
            activeBlock.Image = screenImages.Images[index];
            curActiveBlock = index;
            drawMode = MapDrawMode.Screens;
        }

        private void buttonScrollClick(Object button, EventArgs e)
        {
            int index = ((Button)button).ImageIndex;
            activeBlock.Image = scrollSprites.Images[index];
            curActiveBlock = index;
            drawMode = MapDrawMode.Scrolls;
        }

        private void buttonDoorClick(Object button, EventArgs e)
        {
            int index = ((Button)button).ImageIndex;
            activeBlock.Image = doorSprites.Images[index];
            curActiveBlock = index;
            drawMode = MapDrawMode.Doors;
        }

        private void EditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dirty)
            {
                DialogResult dr = MessageBox.Show("Level was changed. Do you want to save current level?", "Save", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                    saveToFile();
            }
        }

        private bool saveToFile()
        {
            var romFname = "Chip 'n Dale Rescue Rangers (U) [!].nes";
            int width = curLevelLayerData.width;
            int height = curLevelLayerData.height;
            int layerAddr = Globals.levelData[curActiveLevel].getActualLayoutAddr();
            int scrollAddr = Globals.levelData[curActiveLevel].getActualScrollAddr();
            int dirAddr = Globals.levelData[curActiveLevel].getActualDirsAddr();
            for (int i = 0; i < width * height; i++)
            {
                Globals.romdata[layerAddr + i] = curLevelLayerData.layer[i];
                Globals.romdata[scrollAddr + i] = curLevelLayerData.scroll[i];
            }
            for (int i = 0; i < height; i++)
            {
                Globals.romdata[dirAddr + i] = curLevelLayerData.dirs[i];
            }

            //write to file
            try
            {
                using (FileStream f = File.OpenWrite(romFname))
                {
                    f.Write(Globals.romdata, 0, Globals.FILE_SIZE);
                    f.Seek(0, SeekOrigin.Begin);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            dirty = false;
            return true;
        }

        private void returnCbLevelIndex()
        {
            cbLevel.SelectedIndexChanged -= cbLevel_SelectedIndexChanged;
            cbLevel.SelectedIndex = curActiveLevel;
            cbLevel.SelectedIndexChanged += cbLevel_SelectedIndexChanged;
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            saveToFile();
        }

        private void cbShowScrolls_CheckedChanged(object sender, EventArgs e)
        {
            showScrolls = cbShowScrolls.Checked;
            pbMap.Invalidate();
        }

        private void btPreview_Click(object sender, EventArgs e)
        {
            saveToFile();
            var screenList = Globals.buildScreenRecs(curActiveLevel);
            lvObjects.Items.Clear();
            for (int i = 0; i < screenList.Count; i++)
                lvObjects.Items.Add(String.Format("{0:X} ({1}:{2}) [{3:X}]", screenList[i].no, screenList[i].sx, screenList[i].sy, screenList[i].door));
            previewScreens(screenList);
            pbMap.Invalidate();
            objPanel.Invalidate();
        }

        private void btLevelParams_Click(object sender, EventArgs e)
        {
            var f = new EditLevelData();
            f.ShowDialog();
        }
    }
}
