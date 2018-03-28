/*
 * MATLAB Compiler: 6.6 (R2018a)
 * Date: Tue Mar 27 17:32:35 2018
 * Arguments:
 * "-B""macro_default""-W""lib:test""-T""link:lib""-d""C:\Users\Eeg\Documents\MA
 * TLAB\scripts\eegMatlab\compiled\test\for_testing""-v""C:\Users\Eeg\Documents\
 * MATLAB\scripts\eegMatlab\testFunction.m""C:\Users\Eeg\Documents\MATLAB\script
 * s\eegMatlab\testFunction1.m"
 */

#ifndef __test_h
#define __test_h 1

#if defined(__cplusplus) && !defined(mclmcrrt_h) && defined(__linux__)
#  pragma implementation "mclmcrrt.h"
#endif
#include "mclmcrrt.h"
#ifdef __cplusplus
extern "C" {
#endif

/* This symbol is defined in shared libraries. Define it here
 * (to nothing) in case this isn't a shared library. 
 */
#ifndef LIB_test_C_API 
#define LIB_test_C_API /* No special import/export declaration */
#endif

/* GENERAL LIBRARY FUNCTIONS -- START */

extern LIB_test_C_API 
bool MW_CALL_CONV testInitializeWithHandlers(
       mclOutputHandlerFcn error_handler, 
       mclOutputHandlerFcn print_handler);

extern LIB_test_C_API 
bool MW_CALL_CONV testInitialize(void);

extern LIB_test_C_API 
void MW_CALL_CONV testTerminate(void);

extern LIB_test_C_API 
void MW_CALL_CONV testPrintStackTrace(void);

/* GENERAL LIBRARY FUNCTIONS -- END */

/* C INTERFACE -- MLX WRAPPERS FOR USER-DEFINED MATLAB FUNCTIONS -- START */

extern LIB_test_C_API 
bool MW_CALL_CONV mlxTestFunction(int nlhs, mxArray *plhs[], int nrhs, mxArray *prhs[]);

extern LIB_test_C_API 
bool MW_CALL_CONV mlxTestFunction1(int nlhs, mxArray *plhs[], int nrhs, mxArray *prhs[]);

/* C INTERFACE -- MLX WRAPPERS FOR USER-DEFINED MATLAB FUNCTIONS -- END */

/* C INTERFACE -- MLF WRAPPERS FOR USER-DEFINED MATLAB FUNCTIONS -- START */

extern LIB_test_C_API bool MW_CALL_CONV mlfTestFunction(int nargout, mxArray** value);

extern LIB_test_C_API bool MW_CALL_CONV mlfTestFunction1(int nargout, mxArray** outputArg1, mxArray* inputArg1, mxArray* inputArg2);

#ifdef __cplusplus
}
#endif
/* C INTERFACE -- MLF WRAPPERS FOR USER-DEFINED MATLAB FUNCTIONS -- END */

#endif
