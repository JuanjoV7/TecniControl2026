using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.Data.Enums
{
    public enum TipoEquipo
    {
        [Display(Name = "Portátil")]
        Portatil,

        [Display(Name = "Equipo de escritorio")]
        Escritorio,

        [Display(Name = "Todo en uno")]
        TodoEnUno,

        [Display(Name = "Tableta")]
        Tableta,

        [Display(Name = "Otro")]
        Otro
    }
}
