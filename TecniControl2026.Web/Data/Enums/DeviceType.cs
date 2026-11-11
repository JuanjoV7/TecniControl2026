using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.Data.Enums
{
    public enum DeviceType
    {
        [Display(Name = "Portátil")]
        Laptop,

        [Display(Name = "Equipo de escritorio")]
        Desktop,

        [Display(Name = "Todo en uno")]
        AllInOne,

        [Display(Name = "Tableta")]
        Tablet,

        [Display(Name = "Otro")]
        Other
    }
}
