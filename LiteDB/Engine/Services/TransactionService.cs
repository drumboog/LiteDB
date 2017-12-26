﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDB
{
    /// <summary>
    /// Manages all transactions and grantees concurrency and recovery
    /// </summary>
    internal class TransactionService
    {
        private DiskService _disk;
        private AesEncryption _crypto;
        private LockService _locker;
        private PageService _pager;
        private CacheService _cache;
        private Logger _log;

        internal TransactionService(DiskService disk, AesEncryption crypto, PageService pager, LockService locker, CacheService cache, Logger log)
        {
            _disk = disk;
            _crypto = crypto;
            _cache = cache;
            _locker = locker;
            _pager = pager;
            _log = log;
        }

        /// <summary>
        /// Checkpoint is a safe point to clear cache pages without loose pages references.
        /// Is called after each document insert/update/deleted/indexed/fetch from query
        /// Clear only clean pages - do not clear dirty pages (transaction)
        /// Return true if cache was clear
        /// </summary>
        public bool CheckPoint()
        {
            // if (_cache.CleanUsed > _cacheSize)
            // {
            //     _log.Write(Logger.CACHE, "cache size reached {0} pages, will clear now", _cache.CleanUsed);
            // 
            //     _cache.ClearPages();
            // 
            //     return true;
            // }

            return false;
        }

        /// <summary>
        /// Save all dirty pages to disk
        /// </summary>
        public void PersistDirtyPages()
        {
            // get header page
            var header = _pager.GetPage<HeaderPage>(0);

            // increase file changeID (back to 0 when overflow)
            header.ChangeID = header.ChangeID == ushort.MaxValue ? (ushort)0 : (ushort)(header.ChangeID + (ushort)1);

            // mark header as dirty
            _pager.SetDirty(header);

            _log.Write(Logger.DISK, "begin disk operations - changeID: {0}", header.ChangeID);

            // resize file here to fast writes
            _disk.SetLength(BasePage.GetSizeOfPages(header.LastPageID + 1));

            // get all dirty page stating from Header page (SortedList)
            // header page (id=0) always must be first page to write on disk because it's will mark disk as "in recovery"
            foreach (var page in _cache.GetDirtyPages())
            {
                // page.WritePage() updated DiskData with new rendered buffer
                var buffer = _crypto == null || page.PageID == 0 ? 
                    page.WritePage() : 
                    _crypto.Encrypt(page.WritePage());

                _disk.WritePage(page.PageID, buffer);
            }

            // mark all dirty pages as clean pages (all are persisted in disk and are valid pages)
            _cache.MarkDirtyAsClean();

            // flush all data direct to disk
            _disk.Flush();
        }
    }
}