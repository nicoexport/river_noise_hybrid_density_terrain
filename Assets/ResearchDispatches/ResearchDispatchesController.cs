using System.Collections.Generic;
using UnityEngine;

public class ResearchDispatchesController : MonoBehaviour
{


    [Header("Settings")]
    [SerializeField] private float renderDistance = 1024.0f;
    [SerializeField] private float verticalHalfHeight = 256.0f;
    [SerializeField] private float chunkSizeInMeter = 32.0f;
    [SerializeField] private int voxelPerAxis = 16;

    [Header("References")]
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private Transform playerTransform;

    private int kernel;
    private List<Vector3Int> activeChunkOffsets;
    private ComputeBuffer activeChunkOffsetBuffer;
    private GraphicsBuffer drawArgsBuffer;
    private ComputeBuffer TriCountBuffer;

    protected void Start()
    {
        CalculateActiveChunkOffsets();

        SetupBuffers();

        Vector3 centerWorldCoord = new Vector3(playerTransform.position.x, 0.0f, playerTransform.position.z);
        Vector3Int centerChunkCoord = WorldToChunkCoord(centerWorldCoord);

        int voxelPerAxisPadded = voxelPerAxis + 2;
        float voxelSizeInMeter = chunkSizeInMeter / voxelPerAxis;


        kernel = computeShader.FindKernel("CSMain");
        computeShader.SetInt("voxelPerAxis", voxelPerAxisPadded);
        computeShader.SetInt("activeChunkCount", activeChunkOffsets.Count);
        computeShader.SetInts("centerChunkCoord", centerChunkCoord.x, centerChunkCoord.x, centerChunkCoord.z);
        computeShader.SetFloat("chunkSizeInMeter", chunkSizeInMeter);
        computeShader.SetFloat("voxelSizeInMeter", voxelSizeInMeter);


        // upload active chunk offsets to GPU
        // TODO: keep in mind this is only necessary once
        activeChunkOffsetBuffer.SetData(activeChunkOffsets);
        computeShader.SetBuffer(kernel, "activeChunkOffsetBuffer", activeChunkOffsetBuffer);

        int voxelPerChunk = voxelPerAxis * voxelPerAxis * voxelPerAxis;
        int totalVoxelCount = voxelPerChunk * activeChunkOffsets.Count;
        int groupCount = Mathf.CeilToInt(totalVoxelCount / 64);

        computeShader.Dispatch(kernel, groupCount, 1, 1);


        // int triCount =
    }

    private void SetupBuffers()
    {
        activeChunkOffsetBuffer = new ComputeBuffer(activeChunkOffsets.Count, sizeof(int) * 3, ComputeBufferType.Structured);

        drawArgsBuffer = CreateDrawArgsBuffer();
        TriCountBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Raw);
    }

    private void CalculateActiveChunkOffsets()
    {
        activeChunkOffsets = new List<Vector3Int>();

        int radiusInChunks = Mathf.CeilToInt(renderDistance / chunkSizeInMeter);
        int halfHeightInChunks = Mathf.CeilToInt(verticalHalfHeight / chunkSizeInMeter);

        int radiusInChunksSquared = radiusInChunks * radiusInChunks;

        for (int z = -radiusInChunks; z <= radiusInChunks; z++)
        {
            for (int x = -radiusInChunksSquared; x <= radiusInChunksSquared; x++)
            {
                // check if point is outside of circle using square area
                if (x * x + z * z >= radiusInChunksSquared)
                {
                    continue;
                }

                // if it is inside add a offset for each y poisition
                for (int y = -halfHeightInChunks; y <= halfHeightInChunks; y++)
                {
                    activeChunkOffsets.Add(new Vector3Int(x, y, z));
                }
            }
        }
    }

    // HELPERS
    Vector3Int WorldToChunkCoord(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x / chunkSizeInMeter),
            Mathf.FloorToInt(worldPos.y / chunkSizeInMeter),
            Mathf.FloorToInt(worldPos.z / chunkSizeInMeter)
        );
    }

    private GraphicsBuffer CreateDrawArgsBuffer()
    {
        var gb = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawArgs.size);
        gb.SetData(new uint[] { 0, 1, 0, 0 });
        return gb;
    }

    private uint[] _countReadback = new uint[1];
    private int GetAppendCount(ComputeBuffer appendBuffer)
    {
        ComputeBuffer.CopyCount(appendBuffer, TriCountBuffer, 0);
        TriCountBuffer.GetData(_countReadback, 0, 0, 1); // sync readback (ok for debugging)
        return (int)_countReadback[0];
    }
}
