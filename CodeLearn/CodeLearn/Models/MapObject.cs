// CodeLearn\Models\MapObject.cs
// Произвольный визуальный объект на карте курса (SVG-код, PNG/SVG URL, emoji и т.д.)

namespace CodeLearn.Models
{
    public class MapObject
    {
        public int     Id       { get; set; }

        /// <summary>Курс, к которому привязан объект (null = глобальный)</summary>
        public int?    CourseId { get; set; }
        public Course? Course   { get; set; }

        /// <summary>Тип: "svg_inline" | "image_url" | "emoji"</summary>
        public string  Type     { get; set; } = "svg_inline";

        /// <summary>
        /// Для svg_inline  — полный SVG-код (&lt;svg ...&gt;...&lt;/svg&gt;)
        /// Для image_url   — относительный URL (/uploads/map/foo.png) или абсолютный
        /// Для emoji       — один символ или короткая строка
        /// </summary>
        public string  Src      { get; set; } = "";

        /// <summary>Необязательное человеческое название (для списка в Builder)</summary>
        public string  Label    { get; set; } = "";

        // Позиция на карте (% от ширины/высоты канваса — как у LearningModule)
        public double  MapX     { get; set; } = 50;
        public double  MapY     { get; set; } = 50;

        /// <summary>Ширина в px</summary>
        public int     Width    { get; set; } = 48;

        /// <summary>Высота в px. 0 = авто (пропорционально ширине)</summary>
        public int     Height   { get; set; } = 0;

        /// <summary>Инверсия цвета (чёрный↔белый) через CSS filter: invert(1)</summary>
        public bool    Invert   { get; set; } = false;

        /// <summary>Поворот в градусах</summary>
        public double  Rotation { get; set; } = 0;

        /// <summary>CSS z-index (чтобы класть объекты под/над модулями)</summary>
        public int     ZIndex   { get; set; } = 0;
    }
}
