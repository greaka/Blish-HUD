namespace Blish_HUD.Pathing
{
    /// <summary>
    ///     Represents a content-type agnostic name/value pairing.
    /// </summary>
    public struct PathableAttribute
    {
        /// <summary>
        ///     The name of the attribute.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The value of the attribute.
        /// </summary>
        public string Value { get; }

        public PathableAttribute(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Name}={this.Value}";
        }
    }
}