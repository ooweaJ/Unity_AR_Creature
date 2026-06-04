using System;
using Prism.Data;

namespace Prism.Gameplay
{
    /// <summary>
    /// 크리처 생성 파이프라인을 순서대로 실행합니다.
    /// </summary>
    public sealed class GenerationService : IGenerationService
    {
        private readonly CreatureDatabase database;
        private readonly IPixelSampler pixelSampler;
        private readonly ITraitMapper traitMapper;
        private readonly int downsample;

        /// <summary>
        /// 생성 서비스를 구성합니다.
        /// </summary>
        public GenerationService(
            CreatureDatabase database,
            IPixelSampler pixelSampler,
            ITraitMapper traitMapper,
            int downsample = 32)
        {
            this.database = database != null ? database : throw new ArgumentNullException(nameof(database));
            this.pixelSampler = pixelSampler ?? throw new ArgumentNullException(nameof(pixelSampler));
            this.traitMapper = traitMapper ?? throw new ArgumentNullException(nameof(traitMapper));
            this.downsample = Math.Max(1, downsample);
        }

        /// <inheritdoc />
        public CreatureData Generate(in GenerationRequest request)
        {
            ReferenceImageBinding binding = database.FindBinding(request.ReferenceImageName);
            if (binding == null)
            {
                throw new InvalidOperationException($"Reference image binding not found: {request.ReferenceImageName}");
            }

            PixelStats stats = pixelSampler.Sample(request.CameraFrame, request.SampleRegion, downsample);
            int seed = GenerationSeedUtility.Calculate(request.ReferenceImageName, stats);
            return traitMapper.Map(binding, stats, seed);
        }
    }
}
