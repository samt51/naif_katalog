"""Validate the independent project boundary before build/publish smoke tests."""
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]

class FrontendBoundary(unittest.TestCase):
    def test_independent_customer_project_exists(self):
        self.assertTrue((ROOT / 'atasay_katalog.csproj').exists(), 'Independent Atasay frontend is missing')

    def test_no_administrative_controllers_or_views(self):
        self.assertTrue((ROOT / 'Controllers').exists(), 'Customer controllers are missing')
        for name in ('Admin', 'Category', 'Definition', 'HomeContent'):
            self.assertFalse((ROOT / 'Controllers' / (name + 'Controller.cs')).exists())
            self.assertFalse((ROOT / 'Views' / name).exists())

if __name__ == '__main__':
    unittest.main()
