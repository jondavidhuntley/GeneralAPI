using System;

namespace GeneralAPI
{
    /// <summary>
    /// Manage Settings for the Document Store
    /// </summary>
    public interface IDocumentStoreSettings
    {
        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        /// <value>
        /// The endpoint.
        /// </value>
        Uri Endpoint { get; }
    }
}