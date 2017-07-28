using System.Collections.Generic;
using Microsoft.Practices.Unity;

namespace QL4BIMspatial
{
    internal interface ISpatialMain
    {
        void ImportAndReset(IEnumerable<IndexedFaceSet> faceSets);
        void ImportAndReset(IEnumerable<TriangleMesh> meshes);

        ISettings GetSettings();

        void Touch();
        void Overlap();
        void Distance();

        void ArbitraryDirection();
        void AboveOfRelaxed();
        void AboveOfStrict();
        void BelowOfRelaxed();
        void BelowOfStrict();
        void EastOfRelaxed();
        void EastOfStrict();
        void WestofRelaxed();
        void WestofStrict();
        void NorthOfRelaxed();
        void NorthOfStrict();
        void SouthOfRelaxed();
        void SouthOfStrict();

        void Export();
        void ExportTrees();
        void Contain();
        void Equal();
        int GetTriangleCount();
        int CurrentTriangleCount { get; }

    }
}