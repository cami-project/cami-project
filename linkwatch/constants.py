from enum import Enum

# You can find the complete list of supported device types here:
# - https://linkwatchrestservicetest.azurewebsites.net/Help/ResourceModel?modelName=DeviceType
class DeviceType(Enum):
    NOT_FOUND = -1
    PULSE_OXIMETER = 528388
    ECG = 528390
    BLOODPRESSURE = 528391
    TEMPERATURE = 528392
    WEIGHTING_SCALE = 528399
    GLUCOSE_METER = 528401
    INSULIN_PUMP = 528403
    CARDIO = 528425
    STRENGTH = 528426
    PHY_ACT_MON = 528427
    ACTIVITY_HUB = 528455
    MED_MINDER = 528456
    PEAK_FLOW = 528405
    STEP_COUNTER = 528488
    SLEEP_QUALITY = 528408
    FENO = 528489                   # FeNO - Fractional exhaled Nitric Oxide
    HEART_RATE = 528525
    RESPIRATORY_RATE = 528397
    CHOLESTEROL = 528526

# You can find the complete list of supported measurements here:
# - https://linkwatchrestservicetest.azurewebsites.net/Help/ResourceModel?modelName=MeasurementType
class MeasurementType(Enum):
    NOT_FOUND = -1

    PULSE_RATE_NON_INV = 149546
    PULS_OXIM_PULS_RATE = 149530

    SYSTOLIC = 150021
    DIASTOLIC = 150022

    MEAN = 150023
    SATURATION = 150456

    GLUCOSE_WHOLEBLOOD = 160184
    GLUCOSE_PLASMA = 160188

    WEIGHT = 188736
    BODY_FAT = 188748
    BMI = 188752

    FALL_SENSOR = 8519681

    PEF = 152584
    FEV1 = 152586
    FEV6 = 152175
    RESP_RATE = 151562
    HF_SESSION = 8454267
    HF_DISTANCE = 8454247

    HF_STEPS = 8454260
    HF_ENERGY = 8454263
    ATTR_TIME_PD_MSMT_ACTIVE = 68185
    HF_HEARTRATE = 8454258

    SLEEP = 8455148
    FENO = 152587
    CHOLESTEROL = 188737
    TEMPERATURE = 150364

# You can find the complete list of unit codes here:
# - https://linkwatchrestservicetest.azurewebsites.net/Help/ResourceModel?modelName=UnitCode
class UnitCode(Enum):
    NOT_FOUND = -1
    DIMLESS = 262656
    BPM = 264864            # Beats Per Minute
    KILOGRAM = 263875
    CELCIUS = 268192
    MMHG = 266016           # mmHg
    STEP = 268800

# You can find the complete list of context types here:
# - https://linkwatchrestservicetest.azurewebsites.net/Help/ResourceModel?modelName=ContextType
class ContextType(Enum):
    EXERCISE = 29152
    CARB = 29156
    MEDICATION = 29188
    MEAL = 29256
    MEAL_PREPRANDIAL = 29260
    MEAL_POSTPRANDIAL = 29264
    MEAL_FASTING = 29268
    MEAL_CASUAL = 29272
    ACT_REST = 8455145
    ACT_LYING = 8455147
    ACT_SLEEP = 8455148
    ACT_RUN = 8455155
    ACT_WALK = 8455161
    POSITION_SEATED = 8455349
    POSITION_STANDING = 8455350
    POSITION_LYING = 8455355
