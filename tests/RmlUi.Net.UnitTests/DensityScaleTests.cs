using System;
using RmlUiNet;
using Xunit;

namespace RmlUi.Net.UnitTests;

/// <summary>
/// Covers <see cref="Context.SetDensityIndependentPixelRatio"/> and
/// <see cref="Context.GetDensityIndependentPixelRatio"/>.
/// </summary>
/// <remarks>
/// The ratio scales the <c>dp</c> and PPI units and leaves <c>px</c> alone
/// (see <c>KNOWN_ISSUES.md</c>), so a document that declares both units in the same rule set
/// measures the ratio directly.
/// </remarks>
[Collection(RmlUiCollection.Name)]
public sealed class DensityScaleTests : IDisposable
{
    private const string Markup = """
        <rml>
          <head>
            <title>Density</title>
            <style>
              body { font-family: 'LatoLatin'; width: 100%; height: 100%; margin: 0; padding: 0; }
              #pxbox { display: block; width: 200px; height: 50px; }
              #dpbox { display: block; width: 200dp; height: 50dp; }
            </style>
          </head>
          <body>
            <div id="pxbox">px</div>
            <div id="dpbox">dp</div>
          </body>
        </rml>
        """;

    private readonly RmlUiFixture _fixture;
    private readonly DocumentScope _scope;

    public DensityScaleTests(RmlUiFixture fixture)
    {
        _fixture = fixture;
        _scope = fixture.OpenDocument(Markup);
    }

    [Fact]
    public void Default_ratio_is_one()
    {
        Assert.Equal(1f, _scope.Context.GetDensityIndependentPixelRatio(), 3);
    }

    [Theory]
    [InlineData(1.5f)]
    [InlineData(2f)]
    [InlineData(1.25f)]
    public void Dp_lengths_scale_with_the_ratio(float ratio)
    {
        Element box = _scope.Find("dpbox");
        float baseWidth = box.GetOffsetWidth();

        _scope.Context.SetDensityIndependentPixelRatio(ratio);
        _scope.Context.Update();

        Assert.Equal(baseWidth * ratio, box.GetOffsetWidth(), 1);
        Assert.Equal(50f * ratio, box.GetOffsetHeight(), 1);
    }

    [Theory]
    [InlineData(1.5f)]
    [InlineData(2f)]
    public void Px_lengths_do_not_scale_with_the_ratio(float ratio)
    {
        Element box = _scope.Find("pxbox");

        _scope.Context.SetDensityIndependentPixelRatio(ratio);
        _scope.Context.Update();

        // px is a physical unit in RmlUi; only dp and PPI units follow the ratio.
        Assert.Equal(200f, box.GetOffsetWidth(), 1);
        Assert.Equal(50f, box.GetOffsetHeight(), 1);
    }

    [Fact]
    public void Ratio_round_trips_and_restores_the_original_layout()
    {
        Element box = _scope.Find("dpbox");
        float width = box.GetOffsetWidth();

        _scope.Context.SetDensityIndependentPixelRatio(3f);
        _scope.Context.Update();
        Assert.Equal(width * 3f, box.GetOffsetWidth(), 1);

        _scope.Context.SetDensityIndependentPixelRatio(1f);
        _scope.Context.Update();

        Assert.Equal(1f, _scope.Context.GetDensityIndependentPixelRatio(), 3);
        Assert.Equal(width, box.GetOffsetWidth(), 1);
    }

    public void Dispose() => _scope.Dispose();
}
