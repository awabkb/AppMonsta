import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AggregatedRanking } from '../_models/AggregatedRanking';
import { AppDetails } from '../_models/AppDetails';


@Injectable({
  providedIn: 'root'
})
export class AppMonstaService {

  constructor(@Inject('BASE_URL') private baseUrl: string, private http: HttpClient) { }

getAggregatedRankings(store: string, date: string, countryCode: string): Observable<AggregatedRanking[]> {
  const body = { store, date, countryCode };
  return this.http.post<AggregatedRanking[]>(`${this.baseUrl}home/GetAggregatedRankings`, body);
}

  getAppDetails(store: string, countryCode: string, appId: string): Observable<AppDetails> {
    const body = { store, countryCode, appId };
    return this.http.post<AppDetails>(`${this.baseUrl}home/GetAppDetails`, body);
  }

  getAllAppsDetails(store: string, countryCode: string, date: string, genreId: string): Observable<AppDetails[]> {
    const body = { store, countryCode, date, genreId };
    return this.http.post<AppDetails[]>(`${this.baseUrl}home/GetAllAppsDetails`, body);
  }


}
