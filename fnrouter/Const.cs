using System;
using System.Collections.Generic;
using System.Text;

namespace fnrouter
{
    /// <summary>
    /// Константы
    /// </summary>
    static class Const
    {
        public static class FileOpt
        {
            public const string ListFileName = "ListFileName";

            public const string ListFullFileName = "ListFullFileName";
            public const string FullFileName = "FullFileName";
            public const string FileName = "FileName";
            public const string FileWithoutExt = "FileWithoutExt";
            public const string ExtFile = "ExtFile";
            public const string Nalog = "Nalog";
            public const string FileContent = "FileContent";
            /// <summary>
            /// Усеченное имя файла - 8 символов от конца имени и 3 симовла расширения
            /// </summary>
            public const string TruncFileName8d3 = "TruncFileName8d3";
            /// <summary>
            /// Усченное имя файла без расширения
            /// </summary>
            public const string TruncFileWithoutExt = "TruncFileWithoutExt";
            /// <summary>
            /// усеченное расширение
            /// </summary>
            public const string TruncExtFile = "TruncExtFile";

            //public const string TruncFileWithoutExt = "TruncFileWithoutExt";

            public const string DefaultEncoding = "Encoding";

        }
    }
}
