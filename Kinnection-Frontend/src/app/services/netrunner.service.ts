import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable, Type } from '@angular/core';
import { ModifyUsers } from '../models/users';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class NetrunnerService {
  http = inject(HttpClient);
  myHeaders = new Headers({
    'Content-Type': 'application/json',
    Authorization: 'Bearer mytoken',
  });

  get<T>(uri: string, headers?: HttpHeaders) {
    return this.http.get<T>(uri, {
      headers: headers,
      observe: 'response',
      responseType: 'json',
    });
  }

  post<T>(
    uri: string,
    content?: { [key: string]: string },
    headers?: HttpHeaders
  ) {
    return this.http.post<T>(uri, content, {
      headers: headers,
      observe: 'response',
      responseType: 'json',
    });
  }

  put<T>(
    uri: string,
    content?: { [key: string]: string },
    headers?: HttpHeaders
  ) {
    return this.http.put<T>(uri, content, {
      headers: headers,
      observe: 'response',
      responseType: 'json',
    });
  }

  delete<T>(uri: string, headers?: HttpHeaders) {
    return this.http.delete<T>(uri, {
      headers: headers,
      observe: 'response',
      responseType: 'json',
    });
  }

  async test() {
    let get_response = await firstValueFrom(
      this.get('http://localhost:8080/auth/public')
    );
    console.log('Get Public:', get_response.headers.get('X-Public'));

    var id = 0;
    var headers = new HttpHeaders();
    var content = {
      fname: 'string',
      lname: 'string',
      email: 'string',
      password:
        'BEQ2LDlB0+iYZoYobiKpx7OnnoKPGL0a/jLplevCdClYaDemG5vAcnTKnSEZmM2TXJ7kljDUdh8deZqYMl03XXfgPIWi7XtDdR2+M+Hy3JEovc8k+sAfRdmFkDBM01Y7CEuyXNwzQwDhEbTssqvZ/z5UivcjfxPVoOvLgxJj4QMYPIrw/p2f6JArPwQhScRGZpxsLLRs/46CokpRGrMch+HbWSce6s2eJfo0M+FpUGiFtHJIDLLUdK4TOf30i1XeUFAtDgCzo05ACtJxdQu92a3Hek6cnRDzV4hikGRAKOeNOhxrV3s81f/6uur0iiFNW4TL4Q9UiWsGPcR4C8b2LQ==',
    };

    let post_response = await firstValueFrom(
      this.post<ModifyUsers>('http://localhost:8080/users', content)
    );
    id = post_response.body?.id ?? 0;
    headers = post_response.headers;

    console.log('Post id:', id);
    console.log('Post Access:', headers.get('Authorization'));
    console.log('Post Refresh:', headers.get('X-Refresh-Token'));

    let put_response = await firstValueFrom(
      this.put<ModifyUsers>(
        `http://localhost:8080/users/${id}`,
        content,
        headers
      )
    );
    headers = put_response.headers;

    console.log('Put id:', id);
    console.log('Put Access:', headers.get('Authorization'));
    console.log('Put Refresh:', headers.get('X-Refresh-Token'));

    get_response = await firstValueFrom(
      this.get(`http://localhost:8080/users/${id}`, headers)
    );
    headers = get_response.headers;

    console.log('Get Access:', headers.get('Authorization'));
    console.log('Get Refresh:', headers.get('X-Refresh-Token'));

    let delete_response = await firstValueFrom(
      this.delete(`http://localhost:8080/users/${id}`, headers)
    );
    headers = delete_response.headers;

    console.log('Delete Access:', headers.get('Authorization'));
    console.log('Delete Refresh:', headers.get('X-Refresh-Token'));
  }
}
