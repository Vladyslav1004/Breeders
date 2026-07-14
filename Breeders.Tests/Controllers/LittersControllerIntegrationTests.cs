using Breeders.Api.Data;
using Breeders.Api.Enums;
using Breeders.Api.Models.DTOs;
using Breeders.Tests.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Breeders.Tests.Controllers;

public class LittersControllerIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions =
    CreateJsonOptions();

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(
            JsonSerializerDefaults.Web);

        options.Converters.Add(
            new JsonStringEnumConverter());

        return options;
    }

    private static readonly Guid TestBreederId =
    DevelopmentSeedData.FixedIds.PrimaryBreederId;

    private static readonly Guid SecondBreederId =
        DevelopmentSeedData.FixedIds.SecondBreederId;

    private static readonly Guid ApprovedLitterId =
        DevelopmentSeedData.FixedIds.ApprovedLitterId;

    private static readonly Guid DraftLitterId =
        DevelopmentSeedData.FixedIds.DraftLitterId;

    private static readonly Guid OtherBreederLitterId =
        DevelopmentSeedData.FixedIds.OtherBreederLitterId;

    [Fact]
    public async Task GetLitters_WithoutHeader_ReturnsUnauthorized()
    {
        // Arrange
        await using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/litters");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);

        var error = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal(401, error.StatusCode);
        Assert.Equal("unauthorized", error.ErrorCode);
        Assert.Equal(
            "X-Breeder-Id header is required.",
            error.Message);

        Assert.False(
            string.IsNullOrWhiteSpace(error.TraceId));
    }

    [Fact]
    public async Task GetLitters_WithValidHeader_ReturnsCurrentBreederLitters()
    {
        // Arrange
        await using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Breeder-Id",
            TestBreederId.ToString());

        // Act
        var response = await client.GetAsync(
            "/api/litters?pageNumber=1&pageSize=10");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
    .ReadFromJsonAsync<
        PagedResponse<LitterResponse>>(JsonOptions);

        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(1, result.TotalPages);

        Assert.DoesNotContain(
            result.Items,
            item => item.Id == OtherBreederLitterId);
    }

    [Fact]
    public async Task GetLitters_WithStatusFilter_ReturnsFilteredLitters()
    {
        // Arrange
        await using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Breeder-Id",
            TestBreederId.ToString());

        // Act
        var response = await client.GetAsync(
            "/api/litters?status=Draft&pageNumber=1&pageSize=10");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<
            PagedResponse<LitterResponse>>(JsonOptions);

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);

        var litter = result.Items.Single();

        Assert.Equal(DraftLitterId, litter.Id);
        Assert.Equal(LitterStatus.Draft, litter.Status);
    }

    [Fact]
    public async Task GetLitters_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        await using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Breeder-Id",
            TestBreederId.ToString());

        // Act
        var response = await client.GetAsync(
            "/api/litters?pageNumber=1&pageSize=101");

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var error = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal("validation_error", error.ErrorCode);
        Assert.Equal(
            "Page size must be between 1 and 100.",
            error.Message);
    }

    [Fact]
    public async Task Publish_WithValidData_ReturnsPublishedLitter()
    {
        // Arrange
        await using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Breeder-Id",
            TestBreederId.ToString());

        // Act
        var response = await client.PostAsync(
            $"/api/litters/{ApprovedLitterId}/publish",
            content: null);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<LitterResponse>(JsonOptions);

        Assert.NotNull(result);
        Assert.Equal(ApprovedLitterId, result.Id);
        Assert.Equal(
            LitterStatus.Published,
            result.Status);
    }

    [Fact]
    public async Task Publish_WhenBreederIsNotOwner_ReturnsForbidden()
    {
        // Arrange
        await using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Breeder-Id",
            TestBreederId.ToString());

        // Act
        var response = await client.PostAsync(
            $"/api/litters/{OtherBreederLitterId}/publish",
            content: null);

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        var error = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal("forbidden", error.ErrorCode);
        Assert.Equal(
            "You cannot publish another breeder's litter.",
            error.Message);
    }

    [Fact]
    public async Task Publish_WhenLimitIsExceeded_ReturnsConflict()
    {
        // Arrange
        await using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Breeder-Id",
            SecondBreederId.ToString());

        // Act
        var response = await client.PostAsync(
            $"/api/litters/{OtherBreederLitterId}/publish",
            content: null);

        // Assert
        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);

        var error = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal("conflict", error.ErrorCode);
        Assert.Equal(
            "Free publication limit has been exceeded.",
            error.Message);
    }

    [Fact]
    public async Task Publish_WhenLitterDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await using var factory =
            new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Breeder-Id",
            TestBreederId.ToString());

        // Act
        var response = await client.PostAsync(
            $"/api/litters/{Guid.NewGuid()}/publish",
            content: null);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var error = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal("not_found", error.ErrorCode);
        Assert.Equal(
            "Litter was not found.",
            error.Message);
    }
}