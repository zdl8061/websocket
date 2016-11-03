package mx.utils 
{
    import flash.display.*;
    import flash.system.*;
    import mx.core.*;
    
    use namespace mx_internal;
    
    public class LoaderUtil extends Object
    {
        public function LoaderUtil()
        {
            super();
            return;
        }

        public static function normalizeURL(arg1:flash.display.LoaderInfo):String
        {
            var loc2:*=0;
            var loc3:*=null;
            var loc4:*=null;
            var loc1:*=arg1.url;
            var loc5:*=mx.utils.LoaderUtil.mx_internal::urlFilters.length;
            var loc6:*=0;
            while (loc6 < loc5) 
            {
                loc3 = mx.utils.LoaderUtil.mx_internal::urlFilters[loc6].searchString;
                var loc7:*;
                loc2 = loc7 = loc1.indexOf(loc3);
                if (loc7 != -1) 
                {
                    loc1 = (loc4 = mx.utils.LoaderUtil.mx_internal::urlFilters[loc6].filterFunction)(loc1, loc2);
                }
                ++loc6;
            }
            if (isMac()) 
            {
                return encodeURI(loc1);
            }
            return loc1;
        }

        public static function createAbsoluteURL(arg1:String, arg2:String):String
        {
            var loc2:*=0;
            var loc3:*=0;
            var loc1:*=arg2;
            if (arg1 && !(arg2.indexOf(":") > -1 || arg2.indexOf("/") == 0 || arg2.indexOf("\\") == 0)) 
            {
                var loc4:*;
                loc2 = loc4 = arg1.indexOf("?");
                if (loc4 != -1) 
                {
                    arg1 = arg1.substring(0, loc2);
                }
                loc2 = loc4 = arg1.indexOf("#");
                if (loc4 != -1) 
                {
                    arg1 = arg1.substring(0, loc2);
                }
                loc3 = Math.max(arg1.lastIndexOf("\\"), arg1.lastIndexOf("/"));
                if (arg2.indexOf("./") != 0) 
                {
                    while (arg2.indexOf("../") == 0) 
                    {
                        arg2 = arg2.substring(3);
                        loc3 = Math.max(arg1.lastIndexOf("\\", (loc3 - 1)), arg1.lastIndexOf("/", (loc3 - 1)));
                    }
                }
                else 
                {
                    arg2 = arg2.substring(2);
                }
                if (loc3 != -1) 
                {
                    loc1 = arg1.substr(0, loc3 + 1) + arg2;
                }
            }
            return loc1;
        }

        internal static function isMac():Boolean
        {
            return flash.system.Capabilities.os.substring(0, 3) == "Mac";
        }

        internal static function dynamicURLFilter(arg1:String, arg2:int):String
        {
            return arg1.substring(0, arg2);
        }

        internal static function importURLFilter(arg1:String, arg2:int):String
        {
            var loc1:*=arg1.indexOf("://");
            return arg1.substring(0, loc1 + 3) + arg1.substring(arg2 + 12);
        }



        mx_internal static const VERSION:String="4.1.0.16076";

        mx_internal static var urlFilters:Array;
    }
}


