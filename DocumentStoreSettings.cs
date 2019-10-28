using System;

namespace GeneralAPI
{
    /// <summary>
    /// Manage Settings for the Document Store
    /// </summary>
    /// <seealso cref="IDocumentStoreSettings" />
    internal class DocumentStoreSettings : IDocumentStoreSettings
    {
        private readonly string _endpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentStoreSettings"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        public DocumentStoreSettings(string endpoint)
        {
            _endpoint = endpoint;
        }

        Uri IDocumentStoreSettings.Endpoint => new Uri(_endpoint);
    }
}