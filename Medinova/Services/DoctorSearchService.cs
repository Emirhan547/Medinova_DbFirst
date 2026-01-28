using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Elasticsearch.Net;
using Medinova.Dtos;
using Medinova.Models;
using Newtonsoft.Json.Linq;

namespace Medinova.Services
{
    public class DoctorSearchService
    {
        private readonly ElasticLowLevelClient client;
        private readonly string indexName;

        public bool IsEnabled { get; }

        public DoctorSearchService()
        {
            var uriSetting = (ConfigurationManager.AppSettings["Elasticsearch:Uri"] ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(uriSetting))
            {
                IsEnabled = false;
                return;
            }

            var settings = new ConnectionConfiguration(new Uri(uriSetting));
            client = new ElasticLowLevelClient(settings);
            indexName = (ConfigurationManager.AppSettings["Elasticsearch:DoctorsIndex"] ?? "medinova-doctors")
                .Trim()
                .ToLowerInvariant();
            IsEnabled = true;
        }

        public void EnsureSeeded(MedinovaContext context)
        {
            if (!IsEnabled)
            {
                return;
            }

            if (!EnsureIndex())
            {
                return;
            }

            var countResponse = client.Count<StringResponse>(indexName);
            if (!countResponse.Success)
            {
                return;
            }

            var countJson = JObject.Parse(countResponse.Body);
            var total = countJson.Value<long?>("count") ?? 0;
            if (total > 0)
            {
                return;
            }

            var doctors = context.Doctors
                .Select(d => new DoctorSearchDocument
                {
                    DoctorId = d.DoctorId,
                    FullName = d.FullName,
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.Department != null ? d.Department.Name : string.Empty
                })
                .ToList();

            if (doctors.Any())
            {
                IndexDoctors(doctors);
            }
        }

        public IReadOnlyList<DoctorSearchResult> SearchDoctors(string keyword, int? departmentId, int size)
        {
            if (!IsEnabled)
            {
                return null;
            }

            var must = new List<object>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                must.Add(new
                {
                    multi_match = new
                    {
                        query = keyword,
                        fields = new[] { "fullName^2", "departmentName" },
                        fuzziness = "AUTO"
                    }
                });
            }

            if (departmentId.HasValue)
            {
                must.Add(new
                {
                    term = new
                    {
                        departmentId = departmentId.Value
                    }
                });
            }

            object query;

            if (must.Any())
            {
                query = new
                {
                    @bool = new
                    {
                        must
                    }
                };
            }
            else
            {
                query = new
                {
                    match_all = new { }
                };
            }


            var response = client.Search<StringResponse>(indexName, PostData.Serializable(new
            {
                size,
                query
            }));

            if (!response.Success)
            {
                return null;
            }

            var result = new List<DoctorSearchResult>();
            var json = JObject.Parse(response.Body);
            var hits = json["hits"]?["hits"] as JArray;
            if (hits == null)
            {
                return result;
            }

            foreach (var hit in hits)
            {
                var source = hit["_source"];
                if (source == null)
                {
                    continue;
                }

                result.Add(new DoctorSearchResult
                {
                    DoctorId = source.Value<int>("doctorId"),
                    FullName = source.Value<string>("fullName"),
                    DepartmentId = source.Value<int?>("departmentId"),
                    DepartmentName = source.Value<string>("departmentName")
                });
            }

            return result;
        }

        private bool EnsureIndex()
        {
            var existsResponse = client.Indices.Exists<StringResponse>(indexName);
            if (existsResponse.Success && existsResponse.HttpStatusCode == 200)
            {
                return true;
            }

            if (existsResponse.HttpStatusCode.HasValue && existsResponse.HttpStatusCode.Value != 404)
            {
                return false;
            }

            var createResponse = client.Indices.Create<StringResponse>(indexName, PostData.Serializable(new
            {
                mappings = new
                {
                    properties = new
                    {
                        doctorId = new { type = "integer" },
                        fullName = new { type = "text", fields = new { keyword = new { type = "keyword" } } },
                        departmentId = new { type = "integer" },
                        departmentName = new { type = "text", fields = new { keyword = new { type = "keyword" } } }
                    }
                }
            }));

            return createResponse.Success;
        }

        private void IndexDoctors(IEnumerable<DoctorSearchDocument> doctors)
        {
            var bulkPayload = new List<object>();

            foreach (var doctor in doctors)
            {
                bulkPayload.Add(new { index = new { _index = indexName, _id = doctor.DoctorId } });
                bulkPayload.Add(doctor);
            }

            client.Bulk<StringResponse>(PostData.MultiJson(bulkPayload));
        }
    }

}

   
