import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable, Type } from '@angular/core';
import { ModifyUsers } from '../models/users';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class NetrunnerService {
  http = inject(HttpClient);

  get<T>(uri: string, headers?: HttpHeaders) {
    return this.http.get<T>(uri, {
      headers: headers,
      observe: 'response',
      responseType: 'json',
      withCredentials: true
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
      withCredentials: true
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
      withCredentials: true
    });
  }

  delete<T>(uri: string, headers?: HttpHeaders) {
    return this.http.delete<T>(uri, {
      headers: headers,
      observe: 'response',
      responseType: 'json',
      withCredentials: true
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
        'WQldXmZPyzJPeHygAlaFdCoY5x8ZTwST2aZUGAK75FvQ6T/LcLyNoHY2pQaWNFQJ9FmH1wrdCk4njSwMOPbQVZmS30FKAitXhtNHwchbjJHvoOAui33+XfEEBofmPJMMHkFMlEXpIx4XA492TXhXpUf4YO/0VAs3o9qAH2gt0zWZig1xEvAFAlMQPeN0Dt32iHFKFmv7vH6uctjdpyScR945xN64e8hg4X3ZQ0fFVtPVOoLjBh/+AosvB/NZc/9TODtoNyV7DyhxXCmeGRezEpkYjbkyDdNmDqIN3b6fg9hvYabjyJqixNwQnFjtnj5DSYoRBEUCcQ6bXlAYZd4CTQ==',
    };

    let post_response = await firstValueFrom(
      this.post<ModifyUsers>('http://localhost:8080/users', content)
    );
    id = post_response.body?.id ?? 0;

    console.log('Post id:', id);

    let put_response = await firstValueFrom(
      this.put<ModifyUsers>(
        `http://localhost:8080/users/${id}`,
        content
      )
    );

    console.log('Put id:', put_response.body?.id);

    get_response = await firstValueFrom(
      this.get(`http://localhost:8080/users/${id}`, headers)
    );
    headers = get_response.headers;

    console.log('Get Status:', get_response.status);

    let delete_response = await firstValueFrom(
      this.delete(`http://localhost:8080/users/${id}`, headers)
    );
    headers = delete_response.headers;

    console.log('Delete Status:', delete_response.status); 
  }
}
