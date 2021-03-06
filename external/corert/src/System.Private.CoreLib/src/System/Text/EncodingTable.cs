// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Diagnostics;

namespace System.Text
{
    //
    // Data table for encoding classes.  Used by System.Text.Encoding.
    // This class contains two hashtables to allow System.Text.Encoding
    // to retrieve the data item either by codepage value or by webName.
    //

    // Only statics, does not need to be marked with the serializable attribute
    internal static partial class EncodingTable
    {
        /*=================================GetCodePageFromName==========================
        **Action: Given a encoding name, return the correct code page number for this encoding.
        **Returns: The code page for the encoding.
        **Arguments:
        **  name    the name of the encoding
        **Exceptions:
        **  ArgumentNullException if name is null.
        **  internalGetCodePageFromName will throw ArgumentException if name is not a valid encoding name.
        ============================================================================*/

        internal static int GetCodePageFromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return (int)NameToCodePageCache.Instance.GetOrAdd(name);
        }

        //
        // This caches the mapping of an encoding name to a code page.
        //
        private sealed class NameToCodePageCache : ConcurrentUnifier<string, object>
        {
            public static readonly NameToCodePageCache Instance = new NameToCodePageCache();

            protected sealed override object Factory(string name)
            {
                return InternalGetCodePageFromName(name);
            }
        }

        // Find the data item by binary searching the table.
        private static int InternalGetCodePageFromName(string name)
        {
            int left = 0;
            int right = s_encodingNameIndices.Length - 2;
            int index;
            int result;

            Debug.Assert(s_encodingNameIndices.Length == s_codePagesByName.Length + 1);
            Debug.Assert(s_encodingNameIndices[s_encodingNameIndices.Length - 1] == s_encodingNames.Length);

            name = name.ToLowerInvariant();

            //Binary search the array until we have only a couple of elements left and then
            //just walk those elements.
            while ((right - left) > 3)
            {
                index = ((right - left) / 2) + left;

                Debug.Assert(index < s_encodingNameIndices.Length - 1);
                result = CompareOrdinal(name, s_encodingNames, s_encodingNameIndices[index], s_encodingNameIndices[index + 1] - s_encodingNameIndices[index]);
                if (result == 0)
                {
                    //We found the item, return the associated codePage.
                    return (s_codePagesByName[index]);
                }
                else if (result < 0)
                {
                    //The name that we're looking for is less than our current index.
                    right = index;
                }
                else
                {
                    //The name that we're looking for is greater than our current index
                    left = index;
                }
            }

            //Walk the remaining elements (it'll be 3 or fewer).
            for (; left <= right; left++)
            {
                Debug.Assert(left < s_encodingNameIndices.Length - 1);
                if (CompareOrdinal(name, s_encodingNames, s_encodingNameIndices[left], s_encodingNameIndices[left + 1] - s_encodingNameIndices[left]) == 0)
                {
                    return (s_codePagesByName[left]);
                }
            }

            // The encoding name is not valid.
            throw new ArgumentException(
                SR.Format(SR.Argument_EncodingNotSupported, name),
                nameof(name));

        }

        private static int CompareOrdinal(string s1, string s2, int index, int length)
        {
            int count = s1.Length;
            if (count > length)
                count = length;

            int i = 0;
            while (i < count && s1[i] == s2[index + i])
                i++;

            if (i < count)
                return (int)(s1[i] - s2[index + i]);

            return s1.Length - length;
        }

        // Return a list of all EncodingInfo objects describing all of our encodings
        // This is hard coded based on the 
        internal static EncodingInfo[] GetEncodings()
        {
            EncodingInfo[] arrayEncodingInfo = new EncodingInfo[s_mappedCodePages.Length];

            for (int i = 0; i < s_mappedCodePages.Length; i++)
            {
                arrayEncodingInfo[i] = new EncodingInfo(
                    s_mappedCodePages[i],
                    s_webNames.Substring(s_webNameIndices[i], s_webNameIndices[i + 1] - s_webNameIndices[i]),
                    s_englishNames.Substring(s_englishNameIndices[i], s_englishNameIndices[i + 1] - s_englishNameIndices[i])
                    );
            }

            return arrayEncodingInfo;
        }

        internal static CodePageDataItem GetCodePageDataItem(int codePage)
        {
            return (CodePageDataItem)CodePageDataItemCache.Instance.GetOrAdd(codePage);
        }

        //
        // This caches the mapping of a code page to its CodePageDataItem.
        //
        private sealed class CodePageDataItemCache : ConcurrentUnifier<int, object>
        {
            public static readonly CodePageDataItemCache Instance = new CodePageDataItemCache();

            protected sealed override object Factory(int codePage)
            {
                return InternalGetCodePageDataItem(codePage);
            }
        }

        private static CodePageDataItem InternalGetCodePageDataItem(int codePage)
        {
            for (int i = 0; i < s_mappedCodePages.Length; i++)
            {
                if (s_mappedCodePages[i] == codePage)
                {
                    int uiFamilyCodePage = s_uiFamilyCodePages[i];
                    string webName = s_webNames.Substring(s_webNameIndices[i], s_webNameIndices[i + 1] - s_webNameIndices[i]);
                    // All supported code pages have identical web names, header names, and body names.
                    string headerName = webName;
                    string bodyName = webName;
                    string englishName = s_englishNames.Substring(s_englishNameIndices[i], s_englishNameIndices[i + 1] - s_englishNameIndices[i]);
                    uint flags = s_flags[i];

                    return new CodePageDataItem(codePage, uiFamilyCodePage, webName, headerName, bodyName, englishName, flags);
                }
            }

            return null;
        }
    }
}
