using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneralAPI.Model
{
    /// <summary>
    /// Report key Class
    /// </summary>
    public class ReportKey
    {
        /// <summary>
        /// Gets or sets the Report Type or SchemaId
        /// </summary>
        /// <value>Report Type</value>
        public string ReportType { get; set; }

        /// <summary>
        /// Gets or sets the document identifier.
        /// </summary>
        /// <value>
        /// The document identifier.
        /// </value>
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        /// <value>
        /// The currency.
        /// </value>
        [StringLength(3)]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the created.
        /// </summary>
        /// <value>
        /// The created.
        /// </value>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Created { get; set; }
    }
}