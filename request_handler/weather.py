import requests
from os import environ
from dotenv import load_dotenv

load_dotenv()


class Location:
    def __init__(self, latitude: float, longitude: float):
        if latitude < -90 or 90 < latitude or longitude < -180 or 180 < longitude:
            raise ValueError
        self.latitude = latitude
        self.longitude = longitude


class Weather:

    def __init__(self, temperature: float, humidity: float):
        self.temperature = temperature
        self.humidity = humidity


class WeatherAPI:

    @staticmethod
    def get_all(location: Location):
        get_weather_methods = [WeatherAPI.get_weatherbit]
        result = []
        for get_weather in get_weather_methods:
            weather = get_weather(location)
            if weather is not None:
                result.append(weather)
        return result

    @staticmethod
    def get_openweathermap(location: Location) -> Weather:
        pass

    @staticmethod
    def get_weatherbit(location: Location) -> Weather:
        try:
            https_url = 'https://api.weatherbit.io/v2.0/current'
            key = environ['WEATHERBIT_KEY']
            response = requests.get(https_url + f'?key={key}&lat={location.latitude}&lon={location.longitude}').json()
            temperature = response['data'][0]['temp']
            humidity = response['data'][0]['rh']
            return Weather(temperature, humidity)
        except:
            return None
