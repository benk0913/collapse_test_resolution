using System.Collections.Generic;
using Collapse.Blocks;
using UnityEngine;

namespace Collapse
{
    /**
     * Partial class for separating the main functions that are needed to be modified in the context of this test
     */
    public partial class BoardManager
    {
        public const float DELAY_MODIFIER = 0.2f;

        static List<Bomb> BombQueue = new List<Bomb>();


        /**
         * Trigger a bomb
         */
        public void TriggerBomb(Bomb bomb)
        {
            BombQueue.Add(bomb);

            if (BombQueue.Count > 1)
            {
                return;
            }

            TriggerNextBomb();
        }

        public void TriggerNextBomb()
        {
            if (BombQueue.Count == 0)
            {
                ScheduleRegenerateBoard();
                return;
            }

            Bomb bomb = BombQueue[0];
            BombQueue.RemoveAt(0);

            if (bomb.Ignited)
            {
                bomb.OnTriggerComplete = () => CollapseBomb(bomb);
                bomb.Trigger(DELAY_MODIFIER);
            }
            else
            {
                bomb.Shake(() =>
                {
                    bomb.OnTriggerComplete = () => CollapseBomb(bomb);
                    bomb.Trigger(DELAY_MODIFIER);
                });
            }

        }

        void CollapseBomb(Bomb bomb)
        {
            List<Block> blocksToTrigger = new List<Block>();
            for (int x = bomb.GridPosition.x - 1; x <= bomb.GridPosition.x + 1; x++)
            {
                for (int y = bomb.GridPosition.y - 1; y <= bomb.GridPosition.y + 1; y++)
                {
                    if (x < 0 || x >= BoardSize.x) continue;
                    if (y < 0 || y >= BoardSize.y) continue;
                    if (blocks[x, y] == null) continue;
                    if (bomb.GridPosition == new Vector2Int(x, y)) continue;

                    if (blocks[x, y].Type == BlockType.Bomb)
                    {
                        Bomb foundBomb = (Bomb)blocks[x, y];

                        if (!BombQueue.Contains(foundBomb))
                            BombQueue.Add((Bomb)blocks[x, y]);
                    }
                    else
                    {
                        blocksToTrigger.Add(blocks[x, y]);
                    }
                }
            }

            if (blocksToTrigger.Count > 0)
            {
                for (int i = 0; i < blocksToTrigger.Count; i++)
                {
                    if (i == blocksToTrigger.Count - 1)
                    {
                        TriggerMatch(blocksToTrigger[i], TriggerNextBomb);
                    }
                    else
                    {
                        TriggerMatch(blocksToTrigger[i], () => { });
                    }
                }
            }
            else
            {
                TriggerNextBomb();
            }
        }

        /**
         * Trigger a match
         */
        public void TriggerMatch(Block block, System.Action onComplete = null)
        {
            if (block == null)
            {
                onComplete?.Invoke();
                return;
            }

            var results = new List<Block>();
            var tested = new List<(int row, int col)>();

            FindChainRecursive(block.Type, block.GridPosition.x, block.GridPosition.y, tested, results);

            for (int i = 0; i < results.Count; i++)
            {
                if (i == results.Count - 1)
                {
                    if (onComplete == null)
                        ((NormalBlock)results[i]).OnTriggerComplete = ScheduleRegenerateBoard;
                    else
                        ((NormalBlock)results[i]).OnTriggerComplete = onComplete;

                }
                else
                {
                    ((NormalBlock)results[i]).OnTriggerComplete = null;
                }

                results[i].Trigger(DELAY_MODIFIER * i);
            }
        }

        /**
         * Recursively collect all neighbors of same type to build a full list of blocks in this "chain" in the results list
         */
        private void FindChainRecursive(BlockType type, int col, int row, List<(int row, int col)> testedPositions,
            List<Block> results)
        {
            if (testedPositions.Contains((col, row))) return;

            testedPositions.Add((col, row));

            Block existingBlock = blocks[col, row];

            if (existingBlock == null) return;
            if (existingBlock.Type != type) return;

            results.Add(existingBlock);

            if (col - 1 >= 0)
                FindChainRecursive(type, col - 1, row, testedPositions, results);

            if (col + 1 < BoardSize.x)
                FindChainRecursive(type, col + 1, row, testedPositions, results);

            if (row - 1 >= 0)
                FindChainRecursive(type, col, row - 1, testedPositions, results);

            if (row + 1 < BoardSize.y)
                FindChainRecursive(type, col, row + 1, testedPositions, results);
        }
    }
}