package mx.messaging.config 
{
    import flash.display.*;
    import mx.core.*;
    import mx.utils.*;
    
    use namespace mx_internal;
    
    public class LoaderConfig extends Object
    {
        public function LoaderConfig()
        {
            super();
            return;
        }

        public static function init(arg1:flash.display.DisplayObject):void
        {
            if (!mx_internal::_url) 
            {
                mx_internal::_url = mx.utils.LoaderUtil.normalizeURL(arg1.loaderInfo);
                mx_internal::_parameters = arg1.loaderInfo.parameters;
                mx_internal::_swfVersion = arg1.loaderInfo.swfVersion;
            }
            return;
        }

        public static function get parameters():Object
        {
            return mx_internal::_parameters;
        }

        public static function get swfVersion():uint
        {
            return mx_internal::_swfVersion;
        }

        public static function get url():String
        {
            return mx_internal::_url;
        }

        
        

        

        mx_internal static const VERSION:String="4.1.0.16076";

        mx_internal static var _parameters:Object;

        mx_internal static var _swfVersion:uint;

        mx_internal static var _url:String=null;
    }
}


