class ObservationName(object):
    CAMI_WEIGHT = "CAMIWeight"
    CAMI_BLOOD_PRESSURE = "CAMIBloodPresure"

class QuestionnaireID(object):
    CAMI_WEIGHT     = 55
    CAMI_SATURATION = 54
    CAMI_BP         = 53

class MeasurementType(object):
    CAMI_BP         = "332.BP"
    CAMI_PULSE_OXY  = "334.SAT"
    CAMI_WEIGHT     = "336.WEIGHT"

class UnitCode(object):
    BPM         = "BPM"
    KILOGRAM    = "Kilo"
    MMHG        = "mmHg"
    MMOL        = "mmol/L"