namespace gRPC_Receiver.Entity
{
    #region Using
    using System.ComponentModel;
    #endregion Using

    /// <summary>
    /// Тип журнала
    /// </summary>
    public enum RegisterType
    {
        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        [Description("None")]
        None,

        /// <summary>
        /// Аналоговый
        /// </summary>
        [Description("Anlg")]
        Ad,

        /// <summary>
        /// Импульсный
        /// </summary>
        [Description("Tii")]
        Ti,

        /// <summary>
        /// Телесостояние
        /// </summary>
        [Description("Ts")]
        Ts,

        /// <summary>
        /// Значения
        /// </summary>
        [Description("Val")]
        Vd,

        /// <summary>
        /// Контроллер
        /// </summary>
        [Description("Contrler")]
        Contrler
    }
}


