//
// Copyright The Microcks Authors.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0 
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//

var builder = DistributedApplication.CreateBuilder(args);

var microcks = builder.AddMicrocks("microcks")
    .WithMainArtifacts(
        "resources/third-parties/apipastries-openapi.yaml",
        "resources/order-service-openapi.yaml"
    )
    .WithSecondaryArtifacts(
        "resources/order-service-postman-collection.json",
        "resources/third-parties/apipastries-postman-collection.json"
    )
    .WithEnvironment("OTEL_JAVAAGENT_ENABLED", "true")
    .WithOtlpExporter();


var orderapi = builder.AddProject<Projects.Order_Service>("order-api")
// TODO: Improve developer experience by adding specific methods to get mock endpoints
// GetRestMockEndpoint mais aussi GetSoapMockEndpoint, GetAsyncApiMockEndpoint, etc. 
// WithRestMockEndpoint, WithSoapMockEndpoint, WithAsyncApiMockEndpoint, etc. 
// WithEnvironmentFromRestMockEndpoint(string name, string serviceName, string serviceVersion)
// WithEnvironmentFromSoapMockEndpoint(string name, string serviceName, string serviceVersion)
// WithEnvironmentFromAsyncApiMockEndpoint(string name, string serviceName, string serviceVersion)
    .WithEnvironment("PastryApi:BaseUrl", () =>
    {
        // Callback to get the URL once Microcks is started
        var pastryBaseUrl = microcks.Resource.GetRestMockEndpoint("API+Pastries", "0.0.1");

        return pastryBaseUrl.ToString();
    })
    .WaitFor(microcks);

//
// Microcks reference - link the Order API project to Microcks
microcks.WithHostNetworkAccess()
    .WithHostNetworkAccess("order-api")
    .WithHostNetworkAccess("localhost")
    .WithReferenceRelationship(orderapi);

builder.Build().Run();

