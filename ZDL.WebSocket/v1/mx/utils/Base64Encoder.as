package mx.utils 
{
    import flash.utils.*;
    
    public class Base64Encoder extends Object
    {
        public function Base64Encoder()
        {
            this._work = [0, 0, 0];
            super();
            this.reset();
            return;
        }

        public function drain():String
        {
            var loc3:*=null;
            var loc1:*="";
            var loc2:*=0;
            while (loc2 < this._buffers.length) 
            {
                loc3 = this._buffers[loc2] as Array;
                loc1 = loc1 + String.fromCharCode.apply(null, loc3);
                ++loc2;
            }
            this._buffers = [];
            this._buffers.push([]);
            return loc1;
        }

        public function encode(arg1:String, arg2:uint=0, arg3:uint=0):void
        {
            if (arg3 == 0) 
            {
                arg3 = arg1.length;
            }
            var loc1:*=arg2;
            var loc2:*;
            if ((loc2 = arg2 + arg3) > arg1.length) 
            {
                loc2 = arg1.length;
            }
            while (loc1 < loc2) 
            {
                this._work[this._count] = arg1.charCodeAt(loc1);
                var loc3:*;
                var loc4:*=((loc3 = this)._count + 1);
                loc3._count = loc4;
                if (this._count == this._work.length || loc2 - loc1 == 1) 
                {
                    this.encodeBlock();
                    this._count = 0;
                    this._work[0] = 0;
                    this._work[1] = 0;
                    this._work[2] = 0;
                }
                ++loc1;
            }
            return;
        }

        public function encodeUTFBytes(arg1:String):void
        {
            var loc1:*=new flash.utils.ByteArray();
            loc1.writeUTFBytes(arg1);
            loc1.position = 0;
            this.encodeBytes(loc1);
            return;
        }

        public function encodeBytes(arg1:flash.utils.ByteArray, arg2:uint=0, arg3:uint=0):void
        {
            if (arg3 == 0) 
            {
                arg3 = arg1.length;
            }
            var loc1:*=arg1.position;
            arg1.position = arg2;
            var loc2:*=arg2;
            var loc3:*;
            if ((loc3 = arg2 + arg3) > arg1.length) 
            {
                loc3 = arg1.length;
            }
            while (loc2 < loc3) 
            {
                this._work[this._count] = arg1[loc2];
                var loc4:*;
                var loc5:*=((loc4 = this)._count + 1);
                loc4._count = loc5;
                if (this._count == this._work.length || loc3 - loc2 == 1) 
                {
                    this.encodeBlock();
                    this._count = 0;
                    this._work[0] = 0;
                    this._work[1] = 0;
                    this._work[2] = 0;
                }
                ++loc2;
            }
            arg1.position = loc1;
            return;
        }

        public function flush():String
        {
            if (this._count > 0) 
            {
                this.encodeBlock();
            }
            var loc1:*=this.drain();
            this.reset();
            return loc1;
        }

        public function reset():void
        {
            this._buffers = [];
            this._buffers.push([]);
            this._count = 0;
            this._line = 0;
            this._work[0] = 0;
            this._work[1] = 0;
            this._work[2] = 0;
            return;
        }

        public function toString():String
        {
            return this.flush();
        }

        internal function encodeBlock():void
        {
            var loc1:*=this._buffers[(this._buffers.length - 1)] as Array;
            if (loc1.length >= MAX_BUFFER_SIZE) 
            {
                loc1 = [];
                this._buffers.push(loc1);
            }
            loc1.push(ALPHABET_CHAR_CODES[(this._work[0] & 255) >> 2]);
            loc1.push(ALPHABET_CHAR_CODES[(this._work[0] & 3) << 4 | (this._work[1] & 240) >> 4]);
            if (this._count > 1) 
            {
                loc1.push(ALPHABET_CHAR_CODES[(this._work[1] & 15) << 2 | (this._work[2] & 192) >> 6]);
            }
            else 
            {
                loc1.push(ESCAPE_CHAR_CODE);
            }
            if (this._count > 2) 
            {
                loc1.push(ALPHABET_CHAR_CODES[this._work[2] & 63]);
            }
            else 
            {
                loc1.push(ESCAPE_CHAR_CODE);
            }
            if (this.insertNewLines) 
            {
                var loc2:*;
                this._line = loc2 = this._line + 4;
                if (loc2 == 76) 
                {
                    loc1.push(newLine);
                    this._line = 0;
                }
            }
            return;
        }

        
        {
            newLine = 10;
        }

        public static const CHARSET_UTF_8:String="UTF-8";

        public static const MAX_BUFFER_SIZE:uint=32767;

        internal static const ESCAPE_CHAR_CODE:Number=61;

        internal static const ALPHABET_CHAR_CODES:Array=[65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 43, 47];

        public var insertNewLines:Boolean=true;

        internal var _buffers:Array;

        internal var _count:uint;

        internal var _line:uint;

        internal var _work:Array;

        public static var newLine:int=10;
    }
}


