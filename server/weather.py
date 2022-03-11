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


class SimpleWeather:

    def __init__(self, temperature: float, humidity: float):
        self.temperature = temperature
        self.humidity = humidity


class DetailedWeather:

    def __init__(self, temperature, humidity, wind, pressure, cloudiness, description, forecasts=[]):
        self.temperature = temperature
        self.humidity = humidity
        self.wind = wind
        self.pressure = pressure
        self.cloudiness = cloudiness
        self.description = description
        self.forecasts = forecasts

    def get_object_dict(self):
        return self.__dict__


class WeatherAPI:

    @staticmethod
    def get_detailed(location: Location) -> DetailedWeather:
        try:
            key = environ['OPENWEATHERMAP_KEY']
            base_https_url = 'https://api.openweathermap.org/data/2.5/weather?'
            request_url = base_https_url + f'lat={location.latitude}&lon={location.longitude}&appid={key}&units=metric'
            response = requests.get(request_url).json()
            temperature = response['main']['temp']
            humidity = response['main']['humidity']
            wind = response['wind']['speed']
            pressure = response['main']['pressure']
            cloudiness = response['clouds']['all']
            description = response['weather'][0]['description']
            return DetailedWeather(temperature, humidity, wind, pressure, cloudiness, description)
        except:
            return None

    @staticmethod
    def get_all_simple(location: Location, allow_none: bool = False):
        get_weather_methods = [WeatherAPI.get_simple_current_openweathermap, WeatherAPI.get_simple_current_weatherbit]
        result = []
        for get_weather in get_weather_methods:
            weather = get_weather(location)
            if not allow_none:
                continue
            result.append(weather)
        return result

    # https://openweathermap.org/current
    @staticmethod
    def get_simple_current_openweathermap(location: Location) -> SimpleWeather:
        try:
            key = environ['OPENWEATHERMAP_KEY']
            base_https_url = 'https://api.openweathermap.org/data/2.5/weather?'
            request_url = base_https_url + f'lat={location.latitude}&lon={location.longitude}&appid={key}&units=metric'
            response = requests.get(request_url).json()
            temperature = response['main']['temp']
            humidity = response['main']['humidity']
            return SimpleWeather(temperature, humidity)
        except:
            return None

    # https://www.weatherbit.io/api/weather-current
    @staticmethod
    def get_simple_current_weatherapi(location: Location) -> SimpleWeather:
        try:
            key = environ['WEATHERBIT_KEY']
            base_https_url = 'https://api.weatherbit.io/v2.0/current?'
            request_url = base_https_url + f'key={key}&lat={location.latitude}&lon={location.longitude}'
            response = requests.get(request_url).json()
            temperature = response['data'][0]['temp']
            humidity = response['data'][0]['rh']
            return SimpleWeather(temperature, humidity)
        except:
            return None

    # https://www.weatherbit.io/api/weather-current
    @staticmethod
    def get_simple_current_weatherbit(location: Location) -> SimpleWeather:
        try:
            key = environ['WEATHERBIT_KEY']
            base_https_url = 'https://api.weatherbit.io/v2.0/current?'
            request_url = base_https_url + f'key={key}&lat={location.latitude}&lon={location.longitude}'
            response = requests.get(request_url).json()
            temperature = response['data'][0]['temp']
            humidity = response['data'][0]['rh']
            return SimpleWeather(temperature, humidity)
        except:
            return None
