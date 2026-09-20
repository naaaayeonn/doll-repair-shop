using System.Collections.Generic;

namespace DollShop.Core
{
    /// <summary>그날의 의뢰 상태를 추적하는 런타임 컨테이너.</summary>
    internal class OrderRecord
    {
        public Order        Order;
        public bool         Completed;
        public OrderResult? Result;
    }
}
