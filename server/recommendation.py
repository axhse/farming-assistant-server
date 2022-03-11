from time import time
from weather import Location, WeatherAPI
from weather_forecaster import WeatherForecaster


class RecommendationMaker:    # TODO

    # all_recommendation_types = ['Watering', 'Fertilizing', 'Harvest']
    # 86 400 seconds = 1 day
    relevance_periods = {'Watering': 50_000, 'Fertilizing': 100_000, 'Harvest': 500_000}

    @staticmethod
    def get_recommendations(target_field):

        recommendations = []
        latitude, longitude = map(float, target_field['Location'].split())
        weather_list = WeatherAPI.get_all_simple(Location(latitude, longitude))
        weather = WeatherForecaster.get_forecast(weather_list)
        plant = target_field['PlantName']
        planting_date = target_field['PlantingDate']    # unix timestamp

        # TODO
        if weather.temperature == 'something' and '...':
            recommendation_type = 'some type'    # 'Watering', 'Fertilizing' or 'Harvest'
            recommendation_value = 'something'    # 'Harvest it !!! The snow is coming !!'
            relevance_limit_timestamp = round(time()) + RecommendationMaker.relevance_periods[recommendation_type]
            recommendations.append(Recommendation(recommendation_type, recommendation_value, relevance_limit_timestamp))

        # ...

        return recommendations


class Recommendation:

    def __init__(self, recommendation_type: str, value: str, relevance_limit_timestamp: int):
        self.Type = recommendation_type
        self.Value = value
        self.RelevanceLimitTimestamp = relevance_limit_timestamp

    def get_object_dict(self):
        return self.__dict__
