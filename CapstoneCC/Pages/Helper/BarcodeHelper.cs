using BarcodeLib;
using System.Drawing;

public class BarcodeHelper
{
    public static Image GenerateBarcode(string productId)
    {
        Barcode barcode = new Barcode();
        barcode.IncludeLabel = true;
        return barcode.Encode(TYPE.CODE128, productId, Color.Black, Color.White, 300, 150);
    }
}