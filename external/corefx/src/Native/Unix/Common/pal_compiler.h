// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

// These defines are temporary until all files have been migrated from C++ to C
#ifdef __cplusplus
#define BEGIN_EXTERN_C extern "C" {
#define END_EXTERN_C   }
#else
#define BEGIN_EXTERN_C
#define END_EXTERN_C
#endif

#ifndef __has_extension
#define __has_extension(...) 0
#endif

#ifdef static_assert
#define c_static_assert(e) static_assert((e),"")
#elif __has_extension(c_static_assert)
#define c_static_assert(e) _Static_assert((e), "")
#elif __GNUC__ > 4 || (__GNUC__ == 4 && __GNUC_MINOR__ >= 8)
#define c_static_assert(e) typedef char __c_static_assert__[(e)?1:-1]
#else
#define c_static_assert(e)
#endif

#if defined(__sun__) && defined(__svr4__)
#undef static_assert
#define c_static_assert(e)
#endif

#define DLLEXPORT __attribute__ ((__visibility__ ("default")))
