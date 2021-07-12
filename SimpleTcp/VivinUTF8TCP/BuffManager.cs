using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTcp.VivinUTF8TCP
{
    /// <summary>
    /// 任意操作中发现内存超限,会自动清空全部内容
    /// 处理完一笔就清空一笔,里面保持最多一句
    /// </summary>
    class BuffManager
    {
        byte[] _buff;
        int _Counter;
        int MaxSize;
        public BuffManager(int MaxSize)
        {
            this.MaxSize = MaxSize;
            _buff = new byte[MaxSize];
        }
        public void Clear()
        {
            _Counter = 0;
        }

        public int Counter => _Counter;

        public byte[] Data => _buff ;

        internal void Concat(List<byte> data, int idxStart, int cnt)
        {
            if (cnt + _Counter > MaxSize)
            {
                Clear();
            }
            if (cnt > MaxSize)
            {
                throw new OutOfMemoryException("待存储的内容超过最大缓存!");
            }
            data.CopyTo(idxStart, _buff, _Counter,cnt);
            _Counter += cnt;
        }
    }
}
