using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneralAPI.Model
{
    /// <summary>
    /// A report
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the airline.
        /// </summary>
        /// <value>
        /// The airline.
        /// </value>
        [StringLength(3)]
        public string Airline { get; set; }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>
        /// The period.
        /// </value>
        [StringLength(15)]
        public ReportPeriod Period { get; set; }

        /// <summary>
        /// Gets or sets the type of the report.
        /// </summary>
        /// <value>
        /// The type of the report.
        /// </value>
        public string ReportType { get; set; }

        /// <summary>
        /// Gets or sets the created.
        /// </summary>
        /// <value>
        /// The created.
        /// </value>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Created { get; set; }

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
        /// Gets or sets the Exchange Rate used to perform calculations
        /// </summary>
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// Gets or sets and indicator to show if Exchange Rate is Spot or Historic
        /// </summary>
        public bool IsSpotRate { get; set; }

        /// <summary>
        /// Gets or sets the report date.
        /// </summary>
        /// <value>
        /// The report date.
        /// </value>
        public DateTime ReportDate { get; set; }
    }
}