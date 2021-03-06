using CadEditor;
using System;

public class MMUtils 
{ 
  public static ObjRec[] getBlocksLinear2x2MaskPal(int tileId)
  {
      int count = ConfigScript.getBlocksCount(tileId);
      var bb = Utils.readBlocksLinear(Globals.romdata, ConfigScript.getTilesAddr(tileId), 2, 2, count, false, false);
      var palAddr = ConfigScript.getPalBytesAddr(tileId);
      for (int i = 0; i < count; i++)
      {
          bb[i].palBytes[0] = (Globals.romdata[palAddr + i]>>2) & 0x3;
      }
      return bb;
  }
  
  public static void setBlocksLinear2x2MaskPal(int tileId, ObjRec[] blocksData)
  {
    int addr = ConfigScript.getTilesAddr(tileId);
    int count = ConfigScript.getBlocksCount(tileId);
    var palAddr = ConfigScript.getPalBytesAddr(tileId);
    Utils.writeBlocksLinear(blocksData, Globals.romdata, addr, count, false, false);
    for (int i = 0; i < count; i++)
    {
        int t = blocksData[i].palBytes[0];
        Globals.romdata[palAddr + i] = (byte)((Globals.romdata[palAddr + i] & 0xF3)|(t<<2));
    }
  }
}