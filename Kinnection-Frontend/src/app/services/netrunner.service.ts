import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

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
}
