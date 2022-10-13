//Here are only signatures. Implementations should be inside /Modules.
namespace Sean21.OneHMI
{
    public partial class Console
    {
        static partial void _Info(string content);
        public static void Info(string content){
            _Info(content);
        }
        static partial void _Warning(string content);
        public static void Warning(string content){
            _Warning(content);
        }
        static partial void _Error(string content);
        public static void Error(string content){
            _Error(content);
        }
    }
}
