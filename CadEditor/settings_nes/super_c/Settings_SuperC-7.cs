using CadEditor;
using System;
//css_include shared_settings/SharedUtils.cs;

public class Data 
{ 
  public OffsetRec getScreensOffset()  { return new OffsetRec(0x158d7, 14 , 8*8, 8, 8);   }
  
  public bool isBuildScreenFromSmallBlocks() { return true; }
  
  public bool isBigBlockEditorEnabled() { return false; }
  public bool isBlockEditorEnabled()    { return true; }
  public bool isEnemyEditorEnabled()    { return false; }
  
  public OffsetRec getVideoOffset()     { return new OffsetRec(0x0 , 5   , 0x1000);  }
  public OffsetRec getPalOffset  ()     { return new OffsetRec(0x0 , 3   , 16); }
  public GetVideoPageAddrFunc getVideoPageAddrFunc() { return  SharedUtils.fakeVideoAddr(); }
  public GetVideoChunkFunc    getVideoChunkFunc()    { return  SharedUtils.getVideoChunk(new[] {"chr7_000.bin", "chr7_001.bin", "chr7_002.bin", "chr7_003.bin", "chr7_004.bin"}); }
  public SetVideoChunkFunc    setVideoChunkFunc()    { return null; }
  
  public OffsetRec getBlocksOffset()    { return new OffsetRec(0x15c57, 1  , 0x1000);  }
  public int getBlocksCount()           { return 177; }
  public int getBigBlocksCount()        { return 177; }
  public int getPalBytesAddr()          { return 0x16767; }
  public GetBlocksFunc        getBlocksFunc() { return Utils.getBlocksFromTiles16Pal1;}
  public SetBlocksFunc        setBlocksFunc() { return Utils.setBlocksFromTiles16Pal1;}
  
  public GetPalFunc           getPalFunc()           { return  SharedUtils.readPalFromBin(new[] {"pal7_000.bin", "pal7_001.bin", "pal7_002.bin"}); }
  public SetPalFunc           setPalFunc()           { return null;}
}