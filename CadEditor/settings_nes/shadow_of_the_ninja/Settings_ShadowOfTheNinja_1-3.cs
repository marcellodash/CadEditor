using CadEditor;
using System;
//css_include shadow_of_the_ninja/ShadowUtils.cs;

public class Data 
{ 
  public OffsetRec getScreensOffset()  { return new OffsetRec(0x1765e, 10 , 8*8, 8, 8);   }
  
  public bool isBigBlockEditorEnabled() { return true; }
  public bool isBlockEditorEnabled()    { return true; }
  public bool isEnemyEditorEnabled()    { return false; }
  
  public GetVideoPageAddrFunc getVideoPageAddrFunc() { return getVideoAddress; }
  public GetVideoChunkFunc    getVideoChunkFunc()    { return getVideoChunk;   }
  public SetVideoChunkFunc    setVideoChunkFunc()    { return null; }
  
  public OffsetRec getBlocksOffset()    { return new OffsetRec(0x17a5e, 1  , 0x1000);  }
  public int getBlocksCount()           { return 64; }
  public OffsetRec getBigBlocksOffset() { return new OffsetRec(0x17d72, 1  , 0x1000);  }
  public int getBigBlocksCount()        { return 256; }
  
  public GetBlocksFunc        getBlocksFunc() { return ShadowUtils.getBlocks;}
  public SetBlocksFunc        setBlocksFunc() { return ShadowUtils.setBlocks;}
  public GetBigBlocksFunc     getBigBlocksFunc()     { return ShadowUtils.getBigBlocks;}
  public SetBigBlocksFunc     setBigBlocksFunc()     { return ShadowUtils.setBigBlocks;}
  public GetPalFunc           getPalFunc()           { return getPallete;}
  public SetPalFunc           setPalFunc()           { return null;}
  
  //----------------------------------------------------------------------------
  
  public byte[] getPallete(int palId)
  {
      return Utils.readBinFile("pal1-3.bin");
  }
  
  public int getVideoAddress(int id)
  {
    return -1;
  }
  
  public byte[] getVideoChunk(int videoPageId)
  {
     return Utils.readVideoBankFromFile("chr1-3.bin", videoPageId);
  }
}