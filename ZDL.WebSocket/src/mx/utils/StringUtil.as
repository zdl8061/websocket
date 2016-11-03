package mx.utils 
{
    import mx.core.*;
    
    use namespace mx_internal;
    
    public class StringUtil extends Object
    {
        public function StringUtil()
        {
            super();
            return;
        }

        public static function trim(arg1:String):String
        {
            if (arg1 == null) 
            {
                return "";
            }
            var loc1:*=0;
            while (isWhitespace(arg1.charAt(loc1))) 
            {
                ++loc1;
            }
            var loc2:*=(arg1.length - 1);
            while (isWhitespace(arg1.charAt(loc2))) 
            {
                --loc2;
            }
            if (loc2 >= loc1) 
            {
                return arg1.slice(loc1, loc2 + 1);
            }
            return "";
        }

        public static function trimArrayElements(arg1:String, arg2:String):String
        {
            var loc1:*=null;
            var loc2:*=0;
            var loc3:*=0;
            if (!(arg1 == "") && !(arg1 == null)) 
            {
                loc1 = arg1.split(arg2);
                loc2 = loc1.length;
                loc3 = 0;
                while (loc3 < loc2) 
                {
                    loc1[loc3] = mx.utils.StringUtil.trim(loc1[loc3]);
                    ++loc3;
                }
                if (loc2 > 0) 
                {
                    arg1 = loc1.join(arg2);
                }
            }
            return arg1;
        }

        public static function isWhitespace(arg1:String):Boolean
        {
            var loc1:*=arg1;
            switch (loc1) 
            {
                case " ":
                case "\t":
                case "\r":
                case "\n":
                case "":
                {
                    return true;
                }
                default:
                {
                    return false;
                }
            }
        }

        public static function substitute(arg1:String, ... rest):String
        {
            var loc2:*=null;
            if (arg1 == null) 
            {
                return "";
            }
            var loc1:*=rest.length;
            if (loc1 == 1 && rest[0] is Array) 
            {
                loc1 = (loc2 = rest[0] as Array).length;
            }
            else 
            {
                loc2 = rest;
            }
            var loc3:*=0;
            while (loc3 < loc1) 
            {
                arg1 = arg1.replace(new RegExp("\\{" + loc3 + "\\}", "g"), loc2[loc3]);
                ++loc3;
            }
            return arg1;
        }

        public static function repeat(arg1:String, arg2:int):String
        {
            if (arg2 == 0) 
            {
                return "";
            }
            var loc1:*=arg1;
            var loc2:*=1;
            while (loc2 < arg2) 
            {
                loc1 = loc1 + arg1;
                ++loc2;
            }
            return loc1;
        }

        public static function restrict(arg1:String, arg2:String):String
        {
            var loc4:*=0;
            if (arg2 == null) 
            {
                return arg1;
            }
            if (arg2 == "") 
            {
                return "";
            }
            var loc1:*=[];
            var loc2:*=arg1.length;
            var loc3:*=0;
            while (loc3 < loc2) 
            {
                loc4 = arg1.charCodeAt(loc3);
                if (testCharacter(loc4, arg2)) 
                {
                    loc1.push(loc4);
                }
                ++loc3;
            }
            return String.fromCharCode.apply(null, loc1);
        }

        internal static function testCharacter(arg1:uint, arg2:String):Boolean
        {
            var loc7:*=0;
            var loc9:*=false;
            var loc1:*=false;
            var loc2:*=false;
            var loc3:*=false;
            var loc4:*=true;
            var loc5:*=0;
            var loc6:*;
            if ((loc6 = arg2.length) > 0) 
            {
                if ((loc7 = arg2.charCodeAt(0)) == 94) 
                {
                    loc1 = true;
                }
            }
            var loc8:*=0;
            while (loc8 < loc6) 
            {
                loc7 = arg2.charCodeAt(loc8);
                loc9 = false;
                if (loc2) 
                {
                    loc9 = true;
                    loc2 = false;
                }
                else if (loc7 != 45) 
                {
                    if (loc7 != 94) 
                    {
                        if (loc7 != 92) 
                        {
                            loc9 = true;
                        }
                        else 
                        {
                            loc2 = true;
                        }
                    }
                    else 
                    {
                        loc4 = !loc4;
                    }
                }
                else 
                {
                    loc3 = true;
                }
                if (loc9) 
                {
                    if (loc3) 
                    {
                        if (loc5 <= arg1 && arg1 <= loc7) 
                        {
                            loc1 = loc4;
                        }
                        loc3 = false;
                        loc5 = 0;
                    }
                    else 
                    {
                        if (arg1 == loc7) 
                        {
                            loc1 = loc4;
                        }
                        loc5 = loc7;
                    }
                }
                ++loc8;
            }
            return loc1;
        }
        mx_internal static const VERSION:String="4.1.0.16076";
    }
}


