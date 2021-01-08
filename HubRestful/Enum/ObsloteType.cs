namespace HubRestful.Model
{
    /// <summary>
    /// Cache策略类型  永久/绝对过期/滑动过期
    /// </summary>
    public enum ObsloteType
    {
        /// <summary>
        /// 永久
        /// </summary>
        Never,
        /// <summary>
        /// 绝对过期
        /// </summary>
        Absolutely,
        /// <summary>
        /// 滑动过期 如果期间查询或更新，就再次延长
        /// </summary>
        Relative
    }
}