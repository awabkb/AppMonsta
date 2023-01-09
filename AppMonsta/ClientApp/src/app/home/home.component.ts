import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { error, log } from 'console';
import { AggregatedRanking } from '../_models/AggregatedRanking';
import { AppMonstaService } from '../_services/app-monsta.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  private readonly notifier: NotifierService;

 
  rankings!: AggregatedRanking[];
  countries = Object.entries({
      US: "United States",
      AO: 'Angola',
      AE: 'United Arab Emirates',
      FM: 'Micronesia',
      GB: 'United Kingdom',
      GH: 'Ghana',
      GM: 'Gambia',
      GW: 'Guinea-Bissau'
    }
  ).map(([key, value]) => ({ key, value }));

  // Define variables to store the selected values for the filters
  selectedPlatform = 'itunes';
  selectedDate = new Date();
  selectedCountry = 'US';


  constructor(private service: AppMonstaService, public router: Router, notifierService: NotifierService) {
    this.notifier = notifierService;
  }

  ngOnInit() {
    this.service.getAggregatedRankings(this.selectedPlatform, this.selectedDate.toISOString().split('T')[0], this.selectedCountry).subscribe(rankings => {
      this.rankings = rankings;
      this.fetchImages();
    }, err => {
      console.log(err)
      this.notifier.notify('error', err.error.message)
    });
  }
  fetchImages() {
    this.rankings.forEach(ranking => {
      console.log(ranking);
      this.service.getAppDetails(this.selectedPlatform, this.selectedCountry, ranking.ranks[0]).subscribe(app => {
     
        console.log(app);
        ranking.icon_url = app.icon_url;
        }, err => {
          console.log(err)

          this.notifier.notify('error', err.error.message)
        });
    });
  }
  navigateToGenre(genreId: string) {
    this.router.navigate(['/genre', genreId], {
      queryParams: {
        store: this.selectedPlatform,
        date: this.selectedDate.toISOString().split("T")[0],
        countryCode: this.selectedCountry,
        genreId: genreId
      }
    });
  }

  onChange(event:any) {
    this.service.getAggregatedRankings(this.selectedPlatform, this.selectedDate.toISOString().split('T')[0], this.selectedCountry).subscribe(rankings => {
      this.rankings = rankings;
      this.fetchImages();
    }, err => {
      console.log(err)

      this.notifier.notify('error', err.error.message)
    });

  }
}

