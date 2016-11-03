package mx.utils 
{
    import mx.messaging.config.*;
    
    public class URLUtil extends Object
    {
        public function URLUtil()
        {
            super();
            return;
        }

        public static function getServerNameWithPort(arg1:String):String
        {
            var loc1:*=arg1.indexOf("/") + 2;
            var loc2:*=arg1.indexOf("/", loc1);
            return loc2 != -1 ? arg1.substring(loc1, loc2) : arg1.substring(loc1);
        }

        public static function getServerName(arg1:String):String
        {
            var loc1:*=getServerNameWithPort(arg1);
            var loc2:*=loc1.indexOf("]");
            loc2 = loc2 > -1 ? loc1.indexOf(":", loc2) : loc1.indexOf(":");
            if (loc2 > 0) 
            {
                loc1 = loc1.substring(0, loc2);
            }
            return loc1;
        }

        public static function getPort(arg1:String):uint
        {
            var loc4:*=NaN;
            var loc1:*=getServerNameWithPort(arg1);
            var loc2:*=loc1.indexOf("]");
            loc2 = loc2 > -1 ? loc1.indexOf(":", loc2) : loc1.indexOf(":");
            var loc3:*=0;
            if (loc2 > 0) 
            {
                loc4 = Number(loc1.substring(loc2 + 1));
                if (!isNaN(loc4)) 
                {
                    loc3 = int(loc4);
                }
            }
            return loc3;
        }

        public static function getFullURL(arg1:String, arg2:String):String
        {
            var loc1:*=NaN;
            if (!(arg2 == null) && !mx.utils.URLUtil.isHttpURL(arg2)) 
            {
                if (arg2.indexOf("./") == 0) 
                {
                    arg2 = arg2.substring(2);
                }
                if (mx.utils.URLUtil.isHttpURL(arg1)) 
                {
                    if (arg2.charAt(0) != "/") 
                    {
                        loc1 = arg1.lastIndexOf("/") + 1;
                        if (loc1 <= 8) 
                        {
                            arg1 = arg1 + "/";
                            loc1 = arg1.length;
                        }
                    }
                    else 
                    {
                        loc1 = arg1.indexOf("/", 8);
                        if (loc1 == -1) 
                        {
                            loc1 = arg1.length;
                        }
                    }
                    if (loc1 > 0) 
                    {
                        arg2 = arg1.substring(0, loc1) + arg2;
                    }
                }
            }
            return arg2;
        }

        public static function isHttpURL(arg1:String):Boolean
        {
            return !(arg1 == null) && (arg1.indexOf("http://") == 0 || arg1.indexOf("https://") == 0);
        }

        public static function isHttpsURL(arg1:String):Boolean
        {
            return !(arg1 == null) && arg1.indexOf("https://") == 0;
        }

        public static function getProtocol(arg1:String):String
        {
            var loc1:*=arg1.indexOf("/");
            var loc2:*=arg1.indexOf(":/");
            if (loc2 > -1 && loc2 < loc1) 
            {
                return arg1.substring(0, loc2);
            }
            loc2 = arg1.indexOf("::");
            if (loc2 > -1 && loc2 < loc1) 
            {
                return arg1.substring(0, loc2);
            }
            return "";
        }

        public static function replaceProtocol(arg1:String, arg2:String):String
        {
            return arg1.replace(getProtocol(arg1), arg2);
        }

        public static function replacePort(arg1:String, arg2:uint):String
        {
            var loc4:*=0;
            var loc1:*="";
            var loc2:*;
            if ((loc2 = arg1.indexOf("]")) == -1) 
            {
                loc2 = arg1.indexOf(":");
            }
            var loc3:*;
            if ((loc3 = arg1.indexOf(":", loc2 + 1)) > -1) 
            {
                ++loc3;
                loc4 = arg1.indexOf("/", loc3);
                loc1 = arg1.substring(0, loc3) + arg2.toString() + arg1.substring(loc4, arg1.length);
            }
            else if ((loc4 = arg1.indexOf("/", loc2)) > -1) 
            {
                if (arg1.charAt(loc4 + 1) == "/") 
                {
                    loc4 = arg1.indexOf("/", loc4 + 2);
                }
                if (loc4 > 0) 
                {
                    loc1 = arg1.substring(0, loc4) + ":" + arg2.toString() + arg1.substring(loc4, arg1.length);
                }
                else 
                {
                    loc1 = arg1 + ":" + arg2.toString();
                }
            }
            else 
            {
                loc1 = arg1 + ":" + arg2.toString();
            }
            return loc1;
        }

        public static function replaceTokens(arg1:String):String
        {
            var loc3:*=null;
            var loc4:*=null;
            var loc5:*=0;
            var loc1:*=mx.messaging.config.LoaderConfig.url != null ? mx.messaging.config.LoaderConfig.url : "";
            if (arg1.indexOf(SERVER_NAME_TOKEN) > 0) 
            {
                loc3 = mx.utils.URLUtil.getProtocol(loc1);
                loc4 = "localhost";
                if (loc3.toLowerCase() != "file") 
                {
                    loc4 = mx.utils.URLUtil.getServerName(loc1);
                }
                arg1 = arg1.replace(SERVER_NAME_REGEX, loc4);
            }
            var loc2:*=arg1.indexOf(SERVER_PORT_TOKEN);
            if (loc2 > 0) 
            {
                if ((loc5 = mx.utils.URLUtil.getPort(loc1)) > 0) 
                {
                    arg1 = arg1.replace(SERVER_PORT_REGEX, loc5);
                }
                else 
                {
                    if (arg1.charAt((loc2 - 1)) == ":") 
                    {
                        arg1 = arg1.substring(0, (loc2 - 1)) + arg1.substring(loc2);
                    }
                    arg1 = arg1.replace(SERVER_PORT_REGEX, "");
                }
            }
            return arg1;
        }

        public static function urisEqual(arg1:String, arg2:String):Boolean
        {
            if (!(arg1 == null) && !(arg2 == null)) 
            {
                arg1 = mx.utils.StringUtil.trim(arg1).toLowerCase();
                arg2 = mx.utils.StringUtil.trim(arg2).toLowerCase();
                if (arg1.charAt((arg1.length - 1)) != "/") 
                {
                    arg1 = arg1 + "/";
                }
                if (arg2.charAt((arg2.length - 1)) != "/") 
                {
                    arg2 = arg2 + "/";
                }
            }
            return arg1 == arg2;
        }

        public static function hasUnresolvableTokens():Boolean
        {
            return !(mx.messaging.config.LoaderConfig.url == null);
        }

        public static function objectToString(arg1:Object, arg2:String=";", arg3:Boolean=true):String
        {
            var loc1:*;
            return loc1 = internalObjectToString(arg1, arg2, null, arg3);
        }

        internal static function internalObjectToString(arg1:Object, arg2:String, arg3:String, arg4:Boolean):String
        {
            var loc3:*=null;
            var loc4:*=null;
            var loc5:*=null;
            var loc1:*="";
            var loc2:*=true;
            var loc6:*=0;
            var loc7:*=arg1;
            for (loc3 in loc7) 
            {
                if (loc2) 
                {
                    loc2 = false;
                }
                else 
                {
                    loc1 = loc1 + arg2;
                }
                loc4 = arg1[loc3];
                loc5 = arg3 ? arg3 + "." + loc3 : loc3;
                if (arg4) 
                {
                    loc5 = encodeURIComponent(loc5);
                }
                if (loc4 is String) 
                {
                    loc1 = loc1 + (loc5 + "=" + (arg4 ? encodeURIComponent(loc4 as String) : loc4));
                    continue;
                }
                if (loc4 is Number) 
                {
                    loc4 = loc4.toString();
                    if (arg4) 
                    {
                        loc4 = encodeURIComponent(loc4 as String);
                    }
                    loc1 = loc1 + (loc5 + "=" + loc4);
                    continue;
                }
                if (loc4 is Boolean) 
                {
                    loc1 = loc1 + (loc5 + "=" + (loc4 ? "true" : "false"));
                    continue;
                }
                if (loc4 is Array) 
                {
                    loc1 = loc1 + internalArrayToString(loc4 as Array, arg2, loc5, arg4);
                    continue;
                }
                loc1 = loc1 + internalObjectToString(loc4, arg2, loc5, arg4);
            }
            return loc1;
        }

        internal static function internalArrayToString(arg1:Array, arg2:String, arg3:String, arg4:Boolean):String
        {
            var loc5:*=null;
            var loc6:*=null;
            var loc1:*="";
            var loc2:*=true;
            var loc3:*=arg1.length;
            var loc4:*=0;
            while (loc4 < loc3) 
            {
                if (loc2) 
                {
                    loc2 = false;
                }
                else 
                {
                    loc1 = loc1 + arg2;
                }
                loc5 = arg1[loc4];
                loc6 = arg3 + "." + loc4;
                if (arg4) 
                {
                    loc6 = encodeURIComponent(loc6);
                }
                if (loc5 is String) 
                {
                    loc1 = loc1 + (loc6 + "=" + (arg4 ? encodeURIComponent(loc5 as String) : loc5));
                }
                else if (loc5 is Number) 
                {
                    loc5 = loc5.toString();
                    if (arg4) 
                    {
                        loc5 = encodeURIComponent(loc5 as String);
                    }
                    loc1 = loc1 + (loc6 + "=" + loc5);
                }
                else if (loc5 is Boolean) 
                {
                    loc1 = loc1 + (loc6 + "=" + (loc5 ? "true" : "false"));
                }
                else if (loc5 is Array) 
                {
                    loc1 = loc1 + internalArrayToString(loc5 as Array, arg2, loc6, arg4);
                }
                else 
                {
                    loc1 = loc1 + internalObjectToString(loc5, arg2, loc6, arg4);
                }
                ++loc4;
            }
            return loc1;
        }

        public static function stringToObject(arg1:String, arg2:String=";", arg3:Boolean=true):Object
        {
            var loc5:*=null;
            var loc6:*=null;
            var loc7:*=null;
            var loc8:*=null;
            var loc9:*=0;
            var loc10:*=0;
            var loc11:*=null;
            var loc12:*=null;
            var loc13:*=null;
            var loc14:*=null;
            var loc1:*={};
            var loc2:*;
            var loc3:*=(loc2 = arg1.split(arg2)).length;
            var loc4:*=0;
            while (loc4 < loc3) 
            {
                loc6 = (loc5 = loc2[loc4].split("="))[0];
                if (arg3) 
                {
                    loc6 = decodeURIComponent(loc6);
                }
                loc7 = loc5[1];
                if (arg3) 
                {
                    loc7 = decodeURIComponent(loc7 as String);
                }
                if (loc7 != "true") 
                {
                    if (loc7 != "false") 
                    {
                        if ((loc11 = int(loc7)).toString() != loc7) 
                        {
                            if ((loc11 = Number(loc7)).toString() == loc7) 
                            {
                                loc7 = loc11;
                            }
                        }
                        else 
                        {
                            loc7 = loc11;
                        }
                    }
                    else 
                    {
                        loc7 = false;
                    }
                }
                else 
                {
                    loc7 = true;
                }
                loc8 = loc1;
                loc9 = (loc5 = loc6.split(".")).length;
                loc10 = 0;
                while (loc10 < (loc9 - 1)) 
                {
                    loc12 = loc5[loc10];
                    if (loc8[loc12] == null && loc10 < (loc9 - 1)) 
                    {
                        loc13 = loc5[loc10 + 1];
                        if ((loc14 = int(loc13)).toString() != loc13) 
                        {
                            loc8[loc12] = {};
                        }
                        else 
                        {
                            loc8[loc12] = [];
                        }
                    }
                    loc8 = loc8[loc12];
                    ++loc10;
                }
                loc8[loc5[loc10]] = loc7;
                ++loc4;
            }
            return loc1;
        }

        public static const SERVER_NAME_TOKEN:String="{server.name}";

        public static const SERVER_PORT_TOKEN:String="{server.port}";

        internal static const SERVER_NAME_REGEX:RegExp=new RegExp("\\{server.name\\}", "g");

        internal static const SERVER_PORT_REGEX:RegExp=new RegExp("\\{server.port\\}", "g");
    }
}