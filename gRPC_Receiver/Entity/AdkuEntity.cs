namespace gRPC_Receiver.Entity
{
    #region Using
    using System;
    #endregion Using

    /// <summary>
    /// Сущность измененного параметра АДКУ
    /// </summary>
    public class AdkuEntity
    {
        /// <summary>
        /// Тип журнала
        /// </summary>
        public RegisterType RegisterType { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Дата/время
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Дата/время UTC
        /// </summary>
        public DateTime DateTimeUTC { get; set; }

        /// <summary>
        /// Период
        /// </summary>
        public string? TagName { get; set; }
    }
}
