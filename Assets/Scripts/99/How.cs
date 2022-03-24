using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NinetyNine
{
    public class How
    {
        public bool? isSub;
        public int? drawCardIdx;

        public How(bool? isSub, int? drawCardIdx)
        {
            this.isSub = isSub;
            this.drawCardIdx = drawCardIdx;
        }
    }
}
