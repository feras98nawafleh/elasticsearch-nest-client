using AngleSharp.Common;
using Dapper;
using Intalio.CTS.Core.Model;
using Intalio.CTS.Custom.DAL.DTO;
using Intalio.CTS.Custom.DAL.Models;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Intalio.CTS.Custom.DAL.Managers
{
    public class DashboardManager
    {
        private readonly DapperContext _context;
        ILogger<DashboardManager> _logger;
        private readonly ElasticClient _attachmentsElasticClient;

        public DashboardManager(DapperContext context, ILogger<DashboardManager> logger, ElasticManager elasticManager)
        {
            _context = context;
            _logger = logger;
            _attachmentsElasticClient = elasticManager.AttachmentsElasticClient;
        }
        public async Task<DashboardCountsNode> GetCounts(int structureId, string fromDate, string toDate,bool viewAllHierarchy, string language)
        {
            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("StructureId", structureId, DbType.Int32, ParameterDirection.Input);
                parameters.Add("FromDate", fromDate, DbType.DateTime, ParameterDirection.Input);
                parameters.Add("ToDate", toDate, DbType.DateTime, ParameterDirection.Input);
                parameters.Add("ViewAllHierarchy", viewAllHierarchy, DbType.Boolean, ParameterDirection.Input);
                parameters.Add("Language", language, DbType.String, ParameterDirection.Input);


                using (var connection = _context.CreateConnectionCTS())
                {
                    DashboardCountsNode retValFake = new DashboardCountsNode();

                    var lstDashboardCategoriesCounts = await connection.QueryAsync<StatusCounts>("usp_GetDashboardCategoriesCounts", parameters, commandType: CommandType.StoredProcedure);
                    if (lstDashboardCategoriesCounts != null) { 

                        var data = lstDashboardCategoriesCounts.FirstOrDefault();
                        retValFake.CountByStatus = data;
                    }

                    var lstDashboardReceivedClosedCounts = connection.Query<DashboardReceivedClosedCounts>("usp_GetDashboardReceivedClosedCounts", parameters, commandType: CommandType.StoredProcedure).ToList();
                    retValFake.DashboardReceivedClosedCounts = lstDashboardReceivedClosedCounts;

                    var lstStatusCategoriesCounts = connection.Query<StatusCategoriesCounts>("usp_GetCountsByStatusCategories", parameters, commandType: CommandType.StoredProcedure).ToList();
                    retValFake.CountsByStatusCategories = lstStatusCategoriesCounts;

                    return retValFake;
                }
            }
            catch (Exception ex) {}
            return null;
        }
        public async Task<Dictionary<int, double>> GetAverageCompletion(long fromStructureId, long userId, bool isReciever, short privacyLevel, string toStructureId, string yearsSpan)
        {
            var searchDescriptor = new SearchDescriptor<TransferCrawlingModel>()
            .Index("intalio_cts_2")
            .Size(0)
            .Aggregations(agg => agg
                .DateHistogram("created-monthly", dh => dh
                    .Field("createdDate")
                    .CalendarInterval(DateInterval.Month)
                    .Aggregations(innerAgg => innerAgg
                        .Sum("sum_time_difference", sum => sum
                            .Script(script => script
                                .Source("doc['dueDate'].value.toInstant().toEpochMilli() - doc['createdDate'].value.toInstant().toEpochMilli()")
                                .Lang("painless")
                            )
                        )
                        .Cardinality("total_documents", c => c
                            .Field("id")
                        )
                    )
                )
            );

            var searchResponse = await _attachmentsElasticClient.SearchAsync<TransferCrawlingModel>(InjectFilters(fromStructureId, userId, isReciever, privacyLevel, searchDescriptor, toStructureId, yearsSpan));
            Dictionary<int, double> monthAveragePairs = Enumerable.Range(1, 12)
                    .ToDictionary(month => month, _ => 0.0);

            if (searchResponse.Aggregations.DateHistogram("created-monthly").Buckets is not null)
            {
                foreach (var month in searchResponse.Aggregations.DateHistogram("created-monthly").Buckets)
                {
                    if (month.Sum("sum_time_difference").Value > 0)
                        monthAveragePairs[month.Date.Month] = Math.Floor((double)(month.Sum("sum_time_difference").Value / 86400000 / month.Cardinality("total_documents").Value));
                }
            }
            return monthAveragePairs;
        }
        public async Task<Dictionary<int, double>> GetAverageDelay(long fromStructureId, long userId, bool isReciever, short privacyLevel, string toStructureId, string yearsSpan)
        {
            var searchDescriptor = new SearchDescriptor<TransferCrawlingModel>()
            .Index("intalio_cts_2")
            .Size(0)
            .Aggregations(agg => agg
                .DateHistogram("created-monthly", dh => dh
                    .Field("createdDate")
                    .CalendarInterval(DateInterval.Month)
                    .Aggregations(innerAgg => innerAgg
                        .Sum("sum_time_difference", sum => sum
                            .Script(script => script
                                .Source("if (doc['closedDate'].size() > 0) { Math.abs(doc['closedDate'].value.toInstant().toEpochMilli() - doc['dueDate'].value.toInstant().toEpochMilli()) } else { 0 }")
                                .Lang("painless")
                            )
                        )
                        .Cardinality("total_documents", c => c
                            .Field("id")
                        )
                    )
                )
            );

            var searchResponse = await _attachmentsElasticClient.SearchAsync<TransferCrawlingModel>(InjectFilters(fromStructureId, userId, isReciever, privacyLevel, searchDescriptor, toStructureId, yearsSpan));
            Dictionary<int, double> monthAveragePairs = Enumerable.Range(1, 12)
                    .ToDictionary(month => month, _ => 0.0);

            if (searchResponse.Aggregations.DateHistogram("created-monthly").Buckets is not null)
            {
                foreach (var month in searchResponse.Aggregations.DateHistogram("created-monthly").Buckets)
                {
                    if (month.Sum("sum_time_difference").Value > 0)
                        monthAveragePairs[month.Date.Month] = Math.Floor((double)(month.Sum("sum_time_difference").Value / 86400000 / month.Cardinality("total_documents").Value));
                }
            }
            return monthAveragePairs;
        }
        public async Task<Dictionary<string, long>> GetDashboardCounts(long fromStructureId, long userId, bool isReciever, short privacyLevel, string toStructureId, string yearsSpan)
        {
            var todayDate = DateTime.UtcNow.Date;

            string[] dateFilterParts;
            if (yearsSpan is "/")
                yearsSpan = "2015-01-01/2023-12-29";
            dateFilterParts = yearsSpan.Split('/');

            var searchDescriptor = new SearchDescriptor<TransferCrawlingModel>()
                .Index("intalio_cts_2")
                .Size(0)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                  t => t.DateRange(dr => dr.Field("createdDate").GreaterThanOrEquals(DateFormatter(dateFilterParts[0])).LessThanOrEquals(DateFormatter(dateFilterParts[1])))
                            )
                        .Should(
                            sh => sh.Term(t => t.Field(f => f.FromUserId).Value(userId)),
                            sh => sh.Term(t => t.Field(f => f.ToUserId).Value(userId)),
                            sh => sh.Bool(b1 => b1
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToUserId).Value(null)),
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(fromStructureId)),
                                    m => m.Term(t => t.Value(isReciever)),
                                    m => m.Script(s => s.Script(ss => ss.Source("doc['documentPrivacyId'].value <= " + privacyLevel)))
                                )
                            )
                        )
                    )
                )
                .Aggregations(agg => agg
                    .Filter("open_delayed", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .MustNot(mn => mn
                                    .Exists(ex => ex
                                        .Field("closedDate")
                                    )
                                )
                                .Filter(fi => fi
                                    .DateRange(dr => dr
                                        .Field(f => f.DueDate)
                                        .LessThanOrEquals(todayDate)
                                    )
                                )
                            )
                        )
                    )
                    .Filter("open_transfers", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .Must(mu => mu
                                    .Exists(ex => ex
                                        .Field("openedDate")
                                    )
                                )
                            )
                        )
                    )
                    .Filter("open_for_signature", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .MustNot(mn => mn
                                    .Exists(ex => ex
                                        .Field("closedDate")
                                    )
                                )
                                .Filter(fi => fi
                                    .Term(t => t.Field(f => f.PurposeId).Value(8))
                                )
                            )
                        )
                    )
                );

            var searchResponse = await _attachmentsElasticClient.SearchAsync<TransferCrawlingModel>(searchDescriptor);
            Dictionary<string, long> countOpenDelayed = new Dictionary<string, long>
            {
                { "countDelayed", searchResponse.Aggregations.Filter("open_delayed").DocCount },
                { "countOpen", searchResponse.Aggregations.Filter("open_transfers").DocCount },
                { "countForSignature", searchResponse.Aggregations.Filter("open_for_signature").DocCount  }
            };

            return countOpenDelayed;
        }
        public async Task<Dictionary<string, long>> GetReceivedVsClosedCounts(long structureId, long id, bool isStructureReceiver, short privacyLevel, string toStructureId, string yearsSpan)
        {
            string[] dateFilterParts;
            if (yearsSpan is "/")
                yearsSpan = "2015-01-01/2023-12-29";
            dateFilterParts = yearsSpan.Split('/');

            var todayDate = DateTime.UtcNow.Date;
            var searchDescriptor = new SearchDescriptor<TransferCrawlingModel>()
                .Index("intalio_cts_2")
                .Size(0)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                  t => t.DateRange(dr => dr.Field("createdDate").GreaterThanOrEquals(DateFormatter(dateFilterParts[0])).LessThanOrEquals(DateFormatter(dateFilterParts[1])))
                            )
                        .Should(
                            sh => sh.Term(t => t.Field(f => f.FromUserId).Value(id)),
                            sh => sh.Term(t => t.Field(f => f.ToUserId).Value(id)),
                            sh => sh.Bool(b1 => b1
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToUserId).Value(null)),
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(structureId)),
                                    m => m.Term(t => t.Value(isStructureReceiver)),
                                    m => m.Script(s => s.Script(ss => ss.Source("doc['documentPrivacyId'].value <= " + privacyLevel)))
                                )
                            )
                        )
                    )
                )
                .Aggregations(agg => agg
                    .Filter("totalReceived", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .Must(must => must
                                    .Match(match => match
                                        .Field(f => f.ToStructureId)
                                        .Query(toStructureId)
                                    )
                                )
                            )
                        )
                    )
                    .Filter("totalClosed", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                    m => m.Exists(e => e.Field(f => f.ClosedDate))
                                )
                            )
                        )
                    )
                );

            var searchResponse = await _attachmentsElasticClient.SearchAsync<TransferCrawlingModel>(searchDescriptor);
            Dictionary<string, long> countReceivedClosed = new Dictionary<string, long>
            {
                { "Total Received", searchResponse.Aggregations.Filter("totalReceived").DocCount },
                { "Total Closed", searchResponse.Aggregations.Filter("totalClosed").DocCount }
            };

            return countReceivedClosed;
        }
        public async Task<Dictionary<string, long>> GetReceivedCategorized(long structureId, long id, bool isStructureReceiver, short privacyLevel, string toStructureId, string yearsSpan)
        {
            string[] dateFilterParts;
            if (yearsSpan is "/")
                yearsSpan = "2015-01-01/2023-12-29";
            dateFilterParts = yearsSpan.Split('/');

            var searchDescriptor = new SearchDescriptor<TransferCrawlingModel>()
                .Index("intalio_cts_2")
                .Size(0)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                  t => t.DateRange(dr => dr.Field("createdDate").GreaterThanOrEquals(DateFormatter(dateFilterParts[0])).LessThanOrEquals(DateFormatter(dateFilterParts[1])))
                            )
                        .Should(
                            sh => sh.Term(t => t.Field(f => f.FromUserId).Value(id)),
                            sh => sh.Term(t => t.Field(f => f.ToUserId).Value(id)),
                            sh => sh.Bool(b1 => b1
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToUserId).Value(null)),
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(structureId)),
                                    m => m.Term(t => t.Value(isStructureReceiver)),
                                    m => m.Script(s => s.Script(ss => ss.Source("doc['documentPrivacyId'].value <= " + privacyLevel)))
                                )
                            )
                        )
                    )
                )
                .Aggregations(agg => agg
                    .Filter("Incoming", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                    m => m.Term(t => t.Field("documentCategoryId").Value(1))
                                )
                            )
                        )
                    )
                    .Filter("Outgoing", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                    m => m.Term(t => t.Field("documentCategoryId").Value(2))
                                )
                            )
                        )
                    )
                    .Filter("Internal", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                    m => m.Term(t => t.Field("documentCategoryId").Value(3))
                                )
                            )
                        )
                    )
                );

            var searchResponse = await _attachmentsElasticClient.SearchAsync<TransferCrawlingModel>(searchDescriptor);
            Dictionary<string, long> categoriesReceived = new Dictionary<string, long>
            {
                { "Total Incoming", searchResponse.Aggregations.Filter("Incoming").DocCount },
                { "Total Outgoing", searchResponse.Aggregations.Filter("Outgoing").DocCount },
                { "Total Internal", searchResponse.Aggregations.Filter("Internal").DocCount }
            };

            return categoriesReceived;
        }
        public async Task<Dictionary<string, long>> GetClosedCategorized(long structureId, long id, bool isStructureReceiver, short privacyLevel, string toStructureId, string yearsSpan)
        {
            string[] dateFilterParts;
            if (yearsSpan is "/")
                yearsSpan = "2015-01-01/2023-12-29";
            dateFilterParts = yearsSpan.Split('/');

            var searchDescriptor = new SearchDescriptor<TransferCrawlingModel>()
                .Index("intalio_cts_2")
                .Size(0)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                  t => t.DateRange(dr => dr.Field("createdDate").GreaterThanOrEquals(DateFormatter(dateFilterParts[0])).LessThanOrEquals(DateFormatter(dateFilterParts[1])))
                            )
                        .Should(
                            sh => sh.Term(t => t.Field(f => f.FromUserId).Value(id)),
                            sh => sh.Term(t => t.Field(f => f.ToUserId).Value(id)),
                            sh => sh.Bool(b1 => b1
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToUserId).Value(null)),
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(structureId)),
                                    m => m.Term(t => t.Value(isStructureReceiver)),
                                    m => m.Script(s => s.Script(ss => ss.Source("doc['documentPrivacyId'].value <= " + privacyLevel)))
                                )
                            )
                        )
                    )
                )
                .Aggregations(agg => agg
                    .Filter("Incoming", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                    m => m.Term(t => t.Field("documentCategoryId").Value(1)),
                                    m => m.Exists(e => e.Field(f => f.ClosedDate))
                                )
                            )
                        )
                    )
                    .Filter("Outgoing", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                    m => m.Term(t => t.Field("documentCategoryId").Value(2)),
                                    m => m.Exists(e => e.Field(f => f.ClosedDate))
                                )
                            )
                        )
                    )
                    .Filter("Internal", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                    m => m.Term(t => t.Field("documentCategoryId").Value(3)),
                                    m => m.Exists(e => e.Field(f => f.ClosedDate))
                                )
                            )
                        )
                    )
                );

            var searchResponse = await _attachmentsElasticClient.SearchAsync<TransferCrawlingModel>(searchDescriptor);
            Dictionary<string, long> categoriesReceived = new Dictionary<string, long>
            {
                { "Total Closed Incoming", searchResponse.Aggregations.Filter("Incoming").DocCount },
                { "Total Closed Outgoing", searchResponse.Aggregations.Filter("Outgoing").DocCount },
                { "Total Closed Internal", searchResponse.Aggregations.Filter("Internal").DocCount }
            };

            return categoriesReceived;
        }
        public async Task<Dictionary<string, long>> GetOverdueCategorized(long structureId, long id, bool isStructureReceiver, short privacyLevel, string toStructureId, string yearsSpan)
        {
            var todayDate = DateTime.UtcNow.Date;
            string[] dateFilterParts;
            if (yearsSpan is "/")
                yearsSpan = "2015-01-01/2023-12-29";
            dateFilterParts = yearsSpan.Split('/');

            var searchDescriptor = new SearchDescriptor<TransferCrawlingModel>()
                .Index("intalio_cts_2")
                .Size(0)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                  t => t.DateRange(dr => dr.Field("createdDate").GreaterThanOrEquals(DateFormatter(dateFilterParts[0])).LessThanOrEquals(DateFormatter(dateFilterParts[1])))
                            )
                        .Should(
                            sh => sh.Term(t => t.Field(f => f.FromUserId).Value(id)),
                            sh => sh.Term(t => t.Field(f => f.ToUserId).Value(id)),
                            sh => sh.Bool(b1 => b1
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToUserId).Value(null)),
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(structureId)),
                                    m => m.Term(t => t.Value(isStructureReceiver)),
                                    m => m.Script(s => s.Script(ss => ss.Source("doc['documentPrivacyId'].value <= " + privacyLevel)))
                                )
                            )
                        )
                    )
                )
                .Aggregations(agg => agg
                    .Filter("Incoming", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                    m => m.Term(t => t.Field("documentCategoryId").Value(1)),
                                    m => m.DateRange(dr => dr.Field(f => f.DueDate).LessThanOrEquals(todayDate))
                                )
                                .MustNot(
                                    m => m.Exists(e => e.Field(f => f.ClosedDate))
                                )
                            )
                        )
                    )
                    .Filter("Outgoing", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                    m => m.Term(t => t.Field("documentCategoryId").Value(2)),
                                    m => m.DateRange(dr => dr.Field(f => f.DueDate).LessThanOrEquals(todayDate))
                                )
                                .MustNot(
                                    m => m.Exists(e => e.Field(f => f.ClosedDate))
                                )
                            )
                        )
                    )
                    .Filter("Internal", filt => filt
                        .Filter(filter => filter
                            .Bool(b => b
                                .Must(
                                    m => m.Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                                    m => m.Term(t => t.Field("documentCategoryId").Value(3)),
                                    m => m.DateRange(dr => dr.Field(f => f.DueDate).LessThanOrEquals(todayDate))
                                )
                                .MustNot(
                                    m => m.Exists(e => e.Field(f => f.ClosedDate))
                                )
                            )
                        )
                    )
                );

            var searchResponse = await _attachmentsElasticClient.SearchAsync<TransferCrawlingModel>(searchDescriptor);
            Dictionary<string, long> overdueCategorized = new Dictionary<string, long>
            {
                { "Total Overdue Incoming", searchResponse.Aggregations.Filter("Incoming").DocCount },
                { "Total Overdue Outgoing", searchResponse.Aggregations.Filter("Outgoing").DocCount },
                { "Total Overdue Internal", searchResponse.Aggregations.Filter("Internal").DocCount }
            };

            return overdueCategorized;
        }
        private static SearchDescriptor<TransferCrawlingModel> InjectFilters(long structureId, long userId, bool isReciever, short privacyLevel, SearchDescriptor<TransferCrawlingModel> descriptor, string toStructureId, string yearsSpan)
        {
            string[] dateFilterParts;
            if (yearsSpan is "/")
                yearsSpan = "2015-01-01/2023-12-29";

            dateFilterParts = yearsSpan.Split('/');

            descriptor.Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .Term(t => t.Field(f => f.ToStructureId).Value(toStructureId)),
                              t => t.DateRange(dr => dr.Field("createdDate").GreaterThanOrEquals(DateFormatter(dateFilterParts[0])).LessThanOrEquals(DateFormatter(dateFilterParts[1])))
                        )
                    .Should(
                        sh => sh.Term(t => t.Field(f => f.FromUserId).Value(userId)),
                        sh => sh.Term(t => t.Field(f => f.ToUserId).Value(userId)),
                        sh => sh.Bool(b1 => b1
                            .Must(
                                m => m.Term(t => t.Field(f => f.ToUserId).Value(null)),
                                m => m.Term(t => t.Field(f => f.ToStructureId).Value(structureId)),
                                m => m.Term(t => t.Value(isReciever)),
                                m => m.Script(s => s.Script(ss => ss.Source("doc['documentPrivacyId'].value <= " + privacyLevel)))
                            )
                        )
                    )
                )
            );

            return descriptor;
        }
        private static string DateFormatter(string dateToFormat)
        {
            DateTime startDate = DateTime.Parse(dateToFormat);
            return startDate.ToString("yyyy-MM-ddTHH:mm:ss");
        }
    }
}