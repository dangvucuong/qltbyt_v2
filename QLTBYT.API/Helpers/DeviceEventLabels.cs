namespace QLTBYT.API.Helpers;

public static class DeviceEventLabels
{
  public static string GetTypeName(int? typeId) =>
        typeId switch
        {
            1 => "B\u00e1o h\u1ecfng",
            2 => "\u0110\u1ec1 ngh\u1ecb ki\u1ec3m tra",
            3 => "Y\u00eau c\u1ea7u kh\u00e1c",
            4 => "Chuy\u1ec3n giao thi\u1ebft b\u1ecb",
            _ => "Kh\u00e1c"
        };

    public static string GetStatusName(int? statusId) =>
        statusId switch
        {
            1 => "Ch\u1edd x\u1eed l\u00fd",
            2 => "\u0110\u00e3 x\u1eed l\u00fd",
            _ => "Kh\u00f4ng r\u00f5"
        };
}
