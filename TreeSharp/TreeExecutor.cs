using System;
using System.Collections.Generic;

namespace TreeSharp
{
    public class TreeExecutor
    {
        private Composite tree;

        // Constructor
        public TreeExecutor(Composite tree)
        {
            this.tree = tree;
        }

        public void Start()
        {
            // Start MUST be called before you can tick the tree.
            tree.Start(null);
            // do work to spool up a thread, or whatever to call Tick();
        }

        public void Stop()
        {
            tree.Stop(null);
        }

        public void Tick()
        {
            try
            {
                tree.Tick(null);
                // If the last status wasn't running, stop the tree, and restart it.
                if (tree.LastStatus != RunStatus.Running)
                {
                    tree.Stop(null);
                    tree.Start(null);
                }
            }
            catch (Exception e)
            {
                // Restart on any exception.
                tree.Stop(null);
                tree.Start(null);
                throw;
            }
        }
    }
}
