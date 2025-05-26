import { inject, Injectable } from '@angular/core';
import { environment as env } from '../../environments/environment';
import { NetrunnerService } from './netrunner.service';

@Injectable({
  providedIn: 'root',
})
export class KeymasterService {
  crypto = window.crypto;
  subtle = window.crypto.subtle;
  http = inject(NetrunnerService);
  private publicKeyPem: string | null = null;
  private encoder = new TextEncoder();

  // Encrypt text with the imported public key using RSA-OAEP SHA-256
  private async encryptWithRSA_OAEP_SHA256(
    plainText: string,
    base64PublicKey: string
  ): Promise<string> {
    const binaryString = window.atob(base64PublicKey);
    const keyBytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++)
      keyBytes[i] = binaryString.charCodeAt(i);
    const publicKey = await window.crypto.subtle.importKey(
      'spki', // format
      keyBytes.buffer,
      {
        name: 'RSA-OAEP',
        hash: 'SHA-256',
      },
      true, // extractable
      ['encrypt'] // usages
    );
    const data = this.encoder.encode(plainText);
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
      this.http.get(`${env.ISSUER}:${env.ASP_PORT}/auth/public`).subscribe({
        next: (response) => {
          this.publicKeyPem = response.headers.get('X-Public');
        },
        error: () => {
          alert('500 Internal Server Error. Please try again later.');
        },
      });
    }
    return this.encryptWithRSA_OAEP_SHA256(plainText, this.publicKeyPem!);
  }
}
