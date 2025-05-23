import { inject, Injectable } from '@angular/core';
import { environment as env } from '../../environments/environment';
import { NetrunnerService } from './netrunner.service';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class KeymasterService {
  crypto = window.crypto; // access to Web Crypto API
  subtle = crypto.subtle; // shortcut for crypto.subtle

  http = inject(NetrunnerService);
  private publicKeyPem: string | null = null;

  // Helper to convert Base64 string to ArrayBuffer
  private base64ToArrayBuffer(base64: string): ArrayBuffer {
    const binaryString = window.atob(base64);
    const len = binaryString.length;
    const bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes.buffer;
  }

  // Encrypt text with the imported public key using RSA-OAEP SHA-256
  private async encryptWithRSA_OAEP_SHA256(
    plainText: string,
    base64PublicKey: string
  ): Promise<string> {
    const keyBuffer = this.base64ToArrayBuffer(base64PublicKey);
    const publicKey = await window.crypto.subtle.importKey(
      'spki', // format of the public key
      keyBuffer, // the key data
      {
        name: 'RSA-OAEP',
        hash: 'SHA-256',
      },
      true, // extractable
      ['encrypt'] // key usages
    );

    const encoder = new TextEncoder();
    const data = encoder.encode(plainText);
    const encrypted = await window.crypto.subtle.encrypt(
      { name: 'RSA-OAEP' },
      publicKey,
      data
    );
    // Convert encrypted ArrayBuffer to base64 string
    return btoa(String.fromCharCode(...new Uint8Array(encrypted)));
  }

  async encrypt(plainText: string): Promise<string> {
    if (!this.publicKeyPem) {
      let get_response = await firstValueFrom(
        this.http.get(`${env.ISSUER}:${env.ASP_PORT}/auth/public`)
      );
      this.publicKeyPem = get_response.headers.get('X-Public') ?? '';
    }
    return this.encryptWithRSA_OAEP_SHA256(plainText, this.publicKeyPem!);
  }
}
