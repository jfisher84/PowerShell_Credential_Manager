﻿using PSCredentialManager.CredentialManagerApi.Enums;
using PSCredentialManager.CredentialManagerApi.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace PSCredentialManager.CredentialManagerApi.Support
{
    sealed class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid
    {
        internal CriticalCredentialHandle(IntPtr preexistingHandle)
        {
            SetHandle(preexistingHandle);
        }

        internal Credential GetCredential()
        {
            if (!IsInvalid)
            {
                // Get the Credential from the mem location
                NativeCredential nativeCredential = (NativeCredential)Marshal.PtrToStructure(handle,typeof(NativeCredential));

                // Create a managed Credential type and fill it with data from the native counterpart.
                Credential credential = new Credential();
                credential = credential.ConvertToCredential(nativeCredential);
                return credential;
            }
            else
            {
                throw new InvalidOperationException("Invalid CriticalHandle!");
            }
        }

        override protected bool ReleaseHandle()
        {
            // If the handle was set, free it. Return success.
            if (!IsInvalid)
            {
                // NOTE: We should also ZERO out the memory allocated to the handle, before free'ing it
                // so there are no traces of the sensitive data left in memory.
                Imports.CredFree(handle);
                // Mark the handle as invalid for future users.
                SetHandleAsInvalid();
                return true;
            }
            // Return false. 
            return false;
        }

        public Credential[] GetCredentials(int count)
        {
            if (IsInvalid)
            {
                throw new InvalidOperationException("Invalid CriticalHandle!");
            }

            Credential[] Credentials = new Credential[count];
            for (int inx = 0; inx < count; inx++)
            {
                IntPtr pCred = Marshal.ReadIntPtr(handle, inx * IntPtr.Size);
                NativeCredential nativeCredential = (NativeCredential)Marshal.PtrToStructure(pCred, typeof(NativeCredential));
                Credential credential = new Credential();
                credential = credential.ConvertToCredential(nativeCredential);
                Credentials[inx] = credential;
            }
            return Credentials;
        }

    }
}
